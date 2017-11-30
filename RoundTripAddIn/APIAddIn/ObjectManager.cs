using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace RoundTripAddIn
{
   
    public class RunState
    {
        public string key;
        public string value;
        public string reference;
    }
    /* This class deals with serialization of UML Object diagrams to Json */
    public class ObjectManager
    {

        static Logger logger = new Logger();
        static public void setLogger(Logger l)
        {
            logger = l;
        }


        static public Dictionary<string, RunState> parseRunState(String runstate)
        {

            Dictionary<string, RunState> result = new Dictionary<string, RunState>();
            if (runstate == null || runstate.Length == 0)
            {
                return result;
            }

            //string runstatePattern = "@VAR;Variable=(.+);Value=(.+);Note=(.+);Op=(.+);@ENDVAR;";
            string runstatePattern = @"Variable=(?<var>[^;]*);Value=(?<val>[^;]*);Note=(?<note>[^;]*)";
            
            Match m = Regex.Match(runstate, runstatePattern, RegexOptions.IgnoreCase);            
            while (m.Success)
            {
                string variable = m.Result("${var}");
                string value = m.Result("${val}");
                string note = m.Result("${note}");
                if (!result.ContainsKey(variable))
                {
                    RunState rsi = new RunState();
                    rsi.key = variable;
                    rsi.value = value;
                    rsi.reference = note;
                    result.Add(variable, rsi);
                }
                    
                m = m.NextMatch();
            }

            string runstatePattern2 = @"Variable=(?<var>[^;]*);Value=(?<val>[^;]*);Op=(?<op>[^;]*)";
            m = Regex.Match(runstate, runstatePattern2, RegexOptions.IgnoreCase);
            while (m.Success)
            {
                string variable = m.Result("${var}");
                string value = m.Result("${val}");                
                if (!result.ContainsKey(variable))
                {
                    RunState rsi = new RunState();
                    rsi.key = variable;
                    rsi.value = value;                    
                    result.Add(variable, rsi);
                }
                m = m.NextMatch();
            }
            return result;
        }

        public static string renderRunState(Dictionary<string, RunState> values)
        {
            string result = "";
            foreach (string key in values.Keys)
            {
                RunState rs = values[key];
                result += "@VAR;Variable=" + rs.key + ";Value=" + rs.value + ";Note=" + rs.reference + ";Op==;@ENDVAR;";
            }
            return result;
        }
        public static string addRunState(string runstate, string k, string v,int sourceId)
        {

            Dictionary<string,RunState> rsd = parseRunState(runstate);
            if (rsd.ContainsKey(k))
            {
                rsd[k].value = v;
            }
            else
            {
                RunState rs = new RunState();
                rs.key = k;
                rs.value = v;
                rs.reference = "" + sourceId;
                rsd.Add(k,rs);
            }
            return renderRunState(rsd); 
        }


        public static void addRunStateToJson(String rs,JObject jsonClass)
        {
            

            // Loop through all attributes in run state and add to json
            Dictionary<string, RunState> runstate = ObjectManager.parseRunState(rs);
            foreach (string key in runstate.Keys)
            {
                //logger.log("Adding property:" + key + " =>" + runstate[key].value);
                object o = runstate[key].value;

                // Find classifier attribute specified in run state
                string attrType = null;
                string attrUpperBound = null;


                // Add attribute to json as either value or array
                if (attrType != null)
                {
                    //logger.log("  upper bound:" + key + " =>" + attrUpperBound);
                    if (attrUpperBound.Equals("*") || attrUpperBound.Equals(RoundTripAddInClass.CARDINALITY_0_TO_MANY))
                    {
                        // Create array and split values separated by commas
                        JArray ja = new JArray();
                        foreach (string value in runstate[key].value.Split(','))
                        {
                            o = convertEATypeToValue(attrType, value);
                            ja.Add(o);
                        }
                        if(jsonClass.GetValue(key)==null)
                            jsonClass.Add(new JProperty(key, ja));
                    }
                    else
                    {
                        // Not array so convert and add attribute and formatted value
                        o = convertEATypeToValue(attrType, runstate[key].value);
                        //logger.log("Attr:" + attrType + " " + o.ToString());
                        if (jsonClass.GetValue(key) == null)
                            jsonClass.Add(new JProperty(key, o));
                    }
                }
                else
                {
                    // No classifier found so add as object serialized as string
                    //logger.log("Attr:" + key + "-" + o.ToString());
                    if (jsonClass.GetValue(key) == null)
                        jsonClass.Add(new JProperty(key, o));
                }
            }
        }
    
        public static void addTagsToJson(EA.Element el, JObject jsonClass)
        {            
            EA.Collection tagvalues = el.TaggedValues;
            foreach (EA.TaggedValue tv in tagvalues)
            {
                logger.log(tv.Name + ":" + tv.Value);
                if (jsonClass.GetValue(tv.Name) == null)
                    jsonClass.Add(new JProperty(tv.Name, tv.Value));
            }        
        }        


        static public object convertEATypeToValue(string t, string value)
    {
        if (t.Equals(RoundTripAddInClass.EA_TYPE_NUMBER) || t.Equals(RoundTripAddInClass.EA_TYPE_FLOAT))
        {
            try
            {
                return float.Parse(value);
            }
            catch (FormatException e)
            {
                return 0;// "Not a number:"+ value;
            }
        }
        if (t.Equals(RoundTripAddInClass.EA_TYPE_INT))
        {
            try
            {
                return int.Parse(value);
            }
            catch (FormatException)
            {
                return 0;
            }
        }
        else if (t.Equals(RoundTripAddInClass.EA_TYPE_DATE))
        {

            return value;

        }
        else if (t.Equals(RoundTripAddInClass.EA_TYPE_BOOLEAN))
        {
            try
            {
                return bool.Parse(value);
            }
            catch (FormatException)
            {
                return false;
            }

        }
        else if (t.Equals(RoundTripAddInClass.EA_TYPE_DECIMAL))
        {
            try
            {
                return float.Parse(value);
            }
            catch (FormatException)
            {
                return 0;
            }
        }
        else
            return value;
    }


        public static void sync_element_taggedvalue(EA.Repository Repository, EA.Element sample, EA.Element classifier, JObject jo, EA.Package pkg, DiagramCache diagramCache)
        {
            logger.log("Syncing JObject:" + sample.Name);
            EA.Collection taggedValues = sample.TaggedValues;

            if(classifier!=null)
                sample.ClassifierID = classifier.ElementID;

            foreach (JProperty p in jo.Properties())
            {

                if (p.Name == RoundTripAddInClass.POPULATION_PROPERTY_GUID)
                {
                    continue;
                }
                if (p.Name == RoundTripAddInClass.CONSTRAINT_PROPERTY_SOURCE || p.Name == RoundTripAddInClass.CONSTRAINT_PROPERTY_TARGET)
                {
                    continue;
                }
                if (p.Name == RoundTripAddInClass.CONSTRAINT_PROPERTY_CLASS || p.Name == RoundTripAddInClass.CONSTRAINT_PROPERTY_STEREOTYPE)
                {
                    continue;
                }
                if (p.Name == RoundTripAddInClass.POPULATION_PROPERTY_PACKAGE)
                {
                    continue;
                }
                if (p.Name == RoundTripAddInClass.POPULATION_PROPERTY_NAME)
                {
                    sample.Name = p.Value.ToString();
                    continue;
                }
                if (p.Name == RoundTripAddInClass.POPULATION_PROPERTY_NOTES)
                {
                    sample.Notes = p.Value.ToString();
                    continue;
                }


                if (p.Name == RoundTripAddInClass.POPULATION_PROPERTY_TYPE)
                {
                    string classifierName = p.Value.ToString();
                    EA.Element clazz = RepositoryHelper.queryClassifier(Repository, classifierName);
                    if (clazz != null)
                    {
                        sample.ClassifierID = clazz.ElementID;
                        continue;
                    }


                }
                if (p.Value.Type != JTokenType.Object && p.Value.Type != JTokenType.Array)
                {
                    //logger.log("Handling Property:" + p.Name);

                    EA.TaggedValue r = taggedValues.GetByName(p.Name);

                    if (r != null)
                    {
                        //logger.log("Existing Tag");                        
                        r.Value = p.Value.ToString();
                        r.Update();
                    }
                    else
                    {
                        //logger.log("New Tag");                                                
                        EA.TaggedValue tv = sample.TaggedValues.AddNew(p.Name, RoundTripAddInClass.EA_TYPE_STRING);
                        tv.Value = p.Value.ToString();
                        tv.Update();
                    }

                }
            }
            sample.Update();
        }

    }
}
