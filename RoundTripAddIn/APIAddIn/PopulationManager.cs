using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;



namespace RoundTripAddIn
{
    public class PopulationManager
    {
        static Logger logger = new Logger();
        static FileManager fileManager = new FileManager(null);

        static public void setLogger(Logger l)
        {
            logger = l;
        }

        static public void setFileManager(FileManager fm)
        {
            fileManager = fm;
        }

       

        public static void syncPopulation(EA.Repository Repository, EA.Diagram diagram)
        {
            logger.log("Sync Population");
            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagram);

            EA.Element container = container = findContainer(Repository, diagram);
            EA.Element containerClassifierEl = Repository.GetElementByID(container.ClassfierID);
            string containerName = container.Name;
            string containerClassifier = containerClassifierEl.Name;

            EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);
            
        
            string sourcecontrolPackage = RoundTripAddInClass.EXPORT_PACKAGE;


            if (fileManager != null) {
                fileManager.initializeAPI(sourcecontrolPackage);
                fileManager.setDataName(RoundTripAddInClass.POPULATION_PATH);
                fileManager.setup(RoundTripAddInClass.RAML_0_8);
                if (!fileManager.populationExists(container.Name, containerClassifier,RoundTripAddInClass.POPULATION_PATH,container.Name))
                {
                    MessageBox.Show("No file exists at:" + fileManager.exportPath(container.Name, containerClassifier,RoundTripAddInClass.POPULATION_PATH,container.Name));
                    return;
                }
                else
                {
                    string fullpath = fileManager.exportPath(containerName, containerClassifier,RoundTripAddInClass.POPULATION_PATH,container.Name);
                    JArray jo = JArray.Parse(File.ReadAllText(fullpath));
                    sync_population(Repository, container, containerClassifierEl, jo, samplePkg);
                    samplePkg.Update();
                }
            }
        }


        private static void sync_population(EA.Repository Repository, EA.Element sample, EA.Element classifier, JArray ja, EA.Package pkg)
        {
            logger.log("Syncing JArray:" + sample.Name);
            Dictionary<string, RunState> rs = ObjectManager.parseRunState(sample.RunState);
            Dictionary<string, RunState> nrs = new Dictionary<string, RunState>();

            foreach (JObject jo in ja.Children<JObject>())
            {
                logger.log("Syncing Child:" );
                JToken guidToken = null;
                if (jo.TryGetValue(RoundTripAddInClass.POPULATION_PROPERTY_GUID, out guidToken))
                {
                    String guid = guidToken.ToString();
                    EA.Element el = Repository.GetElementByGuid(guid);
                    if (el != null)
                    {
                        //logger.log("Found element for guid" + guid);
                        sync_population(Repository, el, classifier, jo, pkg);
                    } else
                    {
                        logger.log("No element for guid" + guid);
                    }
                } else
                {
                    logger.log("No guid, adding element" + jo.ToString());
                    EA.Element el = pkg.Elements.AddNew("", "Object");
                    logger.log("No guid, adding element" + jo.ToString());
                    sync_population(Repository, el, classifier, jo, pkg);

                }
            }
        }


        private static void sync_population(EA.Repository Repository, EA.Element sample, EA.Element classifier, JObject jo,EA.Package pkg)
        {
            logger.log("Syncing JObject:" + sample.Name);
            Dictionary<string, RunState> rs = ObjectManager.parseRunState(sample.RunState);
            Dictionary<string, RunState> nrs = new Dictionary<string, RunState>();
            
            sample.ClassifierID = classifier.ElementID;

            foreach (JProperty p in jo.Properties())
            {
                if (p.Name == RoundTripAddInClass.POPULATION_PROPERTY_GUID)
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
                //string rsv=null;
                if (p.Value.Type != JTokenType.Object && p.Value.Type != JTokenType.Array)
                {
                    //logger.log("Adding Property:" + sample.Name);
                    RunState r;
                    if (rs.ContainsKey(p.Name)) {
                        r = rs[p.Name];
                    } else {
                        r = new RunState();
                        r.key = p.Name;
                    }
                    r.value = p.Value.ToString();

                    nrs.Add(r.key, r);
                }
            }

            sample.RunState = ObjectManager.renderRunState(nrs);
            //logger.log(sample.RunState);
            sample.Update();

            foreach (EA.Connector con in sample.Connectors)
            {
                logger.log("Connector:" + con.SupplierEnd.Role);
                EA.Element related = null;

                if (sample.ElementID == con.ClientID)
                {
                    related = Repository.GetElementByID(con.SupplierID);

                    JProperty p = jo.Property(con.SupplierEnd.Role);

                    if (p != null)
                    {
                        //logger.log("Found Json Property:" + con.SupplierEnd.Role);
                        if (p.Value.Type == JTokenType.Object)
                        {
                            JObject pjo = (JObject)p.Value;
                            sync_population(Repository, related, classifier, pjo,pkg);
                        }
                        else if (p.Value.Type == JTokenType.Array)
                        {
                            JArray ja = (JArray)p.Value;
                            if (ja.Count > 0)
                            {
                                JToken t = ja.ElementAt(0);
                                ja.RemoveAt(0);
                                if (t.Type == JTokenType.Object)
                                {
                                    sync_population(Repository, related, classifier,(JObject)t,pkg);
                                }
                                else
                                {
                                    MessageBox.Show("Arrays of types other than object not supported");
                                }
                            }
                        }
                    }
                }
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

        static EA.Element findContainer(EA.Repository Repository, EA.Diagram diagram)
        {
            logger.log("Finding container for diagram:" + diagram.Name);

            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagram);
            foreach (EA.Element sample in samples)
            {
                if (sample.Stereotype != null && sample.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_POPULATION)
                {
                    logger.log("Population is identified by POPULATION stereotype");

                    return sample;
                }
            }
            throw new ModelValidationException("Unable to find Object stereotyped as Population on the diagram");
        }


        static public Hashtable sampleToJObject(EA.Repository Repository, EA.Diagram diagram)
        {
            Hashtable result = new Hashtable();

            IList<EA.Element> clazzes = MetaDataManager.diagramClasses(Repository, diagram);

            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagram);

            EA.Element root = findContainer(Repository, diagram);

            EA.Element rootClassifier = Repository.GetElementByID(root.ClassifierID);

            Dictionary<int, JObject> instances = new Dictionary<int, JObject>();
            JArray container = new JArray();
            string containerName = root.Name;
            string containerClassifier = rootClassifier.Name;


            //instances.Add(root.ElementID, container);

            foreach (EA.Element sample in samples)
            {
                //logger.log("Sample Name:" + sample.Name+"\t"+sample.ElementID);

                if (sample.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_POPULATION)
                    continue;

                if (sample.ClassfierID != root.ClassfierID)
                    //skip root elements that are the population elements.
                    continue;

                EA.Element clazz = null;
                if (sample.ClassifierID != 0)
                {
                    clazz = Repository.GetElementByID(sample.ClassifierID);
                } else {
                    logger.log("Classifier is null");
                }

                JObject jsonClass = null;

                {
                    jsonClass = new JObject();
                    jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_GUID, sample.ElementGUID));
                    jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_NAME, sample.Name));
                    jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_NOTES, sample.Notes));
                    if(clazz!=null)
                        jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_TYPE, clazz.Name));

                    container.Add(jsonClass);
                }

                string rs = sample.RunState;

                // Loop through all attributes in run state and add to json
                Dictionary<string, RunState> runstate = ObjectManager.parseRunState(rs);
                foreach (string key in runstate.Keys)
                {
                    logger.log("Adding property:" + key + " =>" + runstate[key].value);
                    object o = runstate[key].value;

                    // Find classifier attribute specified in run state
                    string attrType = null;
                    string attrUpperBound = null;
                    if (clazz != null) {
                        foreach (EA.Attribute a in clazz.Attributes)
                        {
                            if (a.Name.Equals(key))
                            {
                                attrType = a.Type;
                                attrUpperBound = a.UpperBound;
                                break;
                            }
                        }

                        // Check if attribuite is defined as related enumeration. When cardinaltity is 0..* then set the attribute cardinality so we serialize as an array
                        foreach (EA.Connector con in clazz.Connectors)
                        {
                            // Check relation is named the same as the run state attribute name and is an enumeration
                            EA.Element related = Repository.GetElementByID(con.SupplierID);
                            if (con.SupplierEnd.Role == key && related.Type == RoundTripAddInClass.EA_TYPE_ENUMERATION)
                            {
                                //if (con.SupplierEnd.Cardinality.Equals(RoundTripAddInClass.CARDINALITY_0_TO_MANY))
                                //{
                                //logger.log("  matching enum with 0..*:" + con.SupplierEnd.Cardinality);
                                //}
                                attrType = related.Type;
                                attrUpperBound = con.SupplierEnd.Cardinality;
                                break;
                            }
                        }

                        // Check if attribute is defined as related DataItem
                        foreach (EA.Connector con in clazz.Connectors)
                        {
                            // Check relation is named the same as the run state attribute name and is an enumeration
                            EA.Element related = Repository.GetElementByID(con.SupplierID);
                            if (con.SupplierEnd.Role == key && related.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_DATAITEM)
                            {
                                attrType = SchemaManager.getDataItemType(related);
                                attrUpperBound = con.SupplierEnd.Cardinality;
                                break;
                            }
                        }

                    }

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
                            jsonClass.Add(new JProperty(key, ja));
                        }
                        else
                        {
                            // Not array so convert and add attribute and formatted value
                            o = convertEATypeToValue(attrType, runstate[key].value);
                            //logger.log("Attr:" + attrType + " " + o.ToString());
                            jsonClass.Add(new JProperty(key, o));
                        }
                    }
                    else
                    {
                        // No classifier found so add as object serialized as string
                        //logger.log("Attr:" + key + "-" + o.ToString());
                        jsonClass.Add(new JProperty(key, o));
                    }
                }
            }

            logger.log("Export container:" + containerName);

            foreach (EA.Element clazz in samples)
            {

                JObject jsonClass = null;
                if (!instances.TryGetValue(clazz.ElementID, out jsonClass))
                    continue;
                if (jsonClass != null)
                {
                    logger.log("Found jsonClass:" + clazz.Name);
                    foreach (EA.Connector con in clazz.Connectors)
                    {
                        //logger.log("Found connector:");
                        EA.Element related = null;
                        if (clazz.ElementID == con.ClientID)
                        {
                            related = Repository.GetElementByID(con.SupplierID);

                            try
                            {
                                object o = instances[related.ElementID];
                            }
                            catch (KeyNotFoundException)
                            {
                                //Object is in package but not on the diagram
                                continue;
                            }

                            if (related != null && instances[related.ElementID] != null)
                            {

                                if (con.SupplierEnd.Cardinality.Equals(RoundTripAddInClass.CARDINALITY_0_TO_MANY) ||
                                    con.SupplierEnd.Cardinality.Equals(RoundTripAddInClass.CARDINALITY_1_TO_MANY)
                                )
                                {
                                    //logger.log("Found array");

                                    string propertyName = related.Name;
                                    //Override with the connection supplier end
                                    try {
                                        if (con.SupplierEnd.Role.Length > 0)
                                            propertyName = con.SupplierEnd.Role;
                                    } catch (Exception) { }

                                    JProperty p = jsonClass.Property(propertyName);
                                    if (p == null) {
                                        JArray ja = new JArray();
                                        ja.Add(instances[related.ElementID]);
                                        //logger.log("Adding array property:"+ related.Name);   
                                        jsonClass.Add(new JProperty(propertyName, ja));
                                    } else {
                                        JArray ja = (JArray)p.Value;
                                        //logger.log("Adding to array property");   
                                        ja.Add(instances[related.ElementID]);
                                    }
                                }
                                else
                                {
                                    string propertyName = related.Name;
                                    //Override with the connection supplier end
                                    try {
                                        if (con.SupplierEnd.Role.Length > 0)
                                            propertyName = con.SupplierEnd.Role;
                                    } catch (Exception) { }
                                    //logger.log("Adding property:" + related.Name);
                                    jsonClass.Add(new JProperty(propertyName, instances[related.ElementID]));
                                }

                            }
                        }
                    }
                }
            }

            //KeyValuePair<string,JObject> kv = new KeyValuePair<string,JObject>(containerName,container);            
            //return kv;

            //logger.log("REturning result");
            result.Add("sample", containerName);
            result.Add("class", containerClassifier);            
            result.Add("json", container);
            return result;
        }

        static public void exportPopulation(EA.Repository Repository, EA.Diagram diagram)
        {
            try
            {
                if (!diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_POPULATIONDIAGRAM))
                {
                    logger.log("exportSample: Ignore diagam that isnt a population diagram");
                    return;
                }

                Hashtable ht = sampleToJObject(Repository, diagram);
                string sample = (string)ht["sample"];
                string clazz = (string)ht["class"];                
                JArray container = (JArray)ht["json"];


                logger.log("Population Size:" + container.Count);
                //KeyValuePair<string,JObject> kv = sampleToJObject(Repository, diagram);
                //JObject container = kv.Value;

                if (container == null)
                {
                    MessageBox.Show("No object linked to root with classification sample declared nor  (older style) object of classification Request declared");
                    return;
                }

                string msg = JsonConvert.SerializeObject(container, Newtonsoft.Json.Formatting.Indented) + "\n";
                EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);
                EA.Package samplesPackage = Repository.GetPackageByID(samplePkg.ParentID);
                EA.Package apiPackage = Repository.GetPackageByID(samplesPackage.ParentID);

                string sourcecontrolPackage = apiPackage.Name;
                if (MetaDataManager.isCDMPackage(Repository, apiPackage))
                {
                    sourcecontrolPackage = "cdm";
                }

                sourcecontrolPackage = RoundTripAddInClass.EXPORT_PACKAGE;

                if (fileManager != null)
                {
                    fileManager.initializeAPI(sourcecontrolPackage);
                    fileManager.setDataName(RoundTripAddInClass.POPULATION_PATH);
                    fileManager.setup(RoundTripAddInClass.RAML_0_8);
                    fileManager.exportData(sample, clazz, msg,RoundTripAddInClass.POPULATION_PATH,sample);                    
                }           
          }catch(ModelValidationException ex){
            MessageBox.Show(ex.errors.messages.ElementAt(0).ToString());
          }            
        }        

        ///
        /// Validate all object run state keys correspond to classifier attributes
        ///
        static public void validateDiagram(EA.Repository Repository,EA.Diagram diagram)
        {                        
            IList<string> messages = diagramValidation(Repository,diagram);

            logger.log("**ValidationResults**");
            if(messages!=null)
            {                
                foreach (string m in messages)
                {
                    logger.log(m);                    
                }                                
            }                        
        }

        static public IList<string> diagramValidation(EA.Repository Repository, EA.Diagram diagram)
        {
            JSchema jschema = null;
            JObject json = null;
            try
            {
                //logger.log("Validate Sample");
                json = (JObject)sampleToJObject(Repository, diagram)["json"];

                //logger.log("JObject formed");
            
                EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);            
                EA.Package samplesPackage = Repository.GetPackageByID(samplePkg.ParentID);            
                EA.Package apiPackage = Repository.GetPackageByID(samplesPackage.ParentID);
            
                EA.Package schemaPackage = null;
            
                foreach (EA.Package p in apiPackage.Packages)
                {                
                    if (p!=null && p.Name.Equals(RoundTripAddInClass.API_PACKAGE_SCHEMAS))
                    {
                        schemaPackage = p;
                    }
                }
                if (schemaPackage == null)
                {
                    throw new Exception("No Schema package found");                
                }
                            
                EA.Diagram schemaDiagram = null;            
                foreach (EA.Diagram d in schemaPackage.Diagrams)
                {
                    if (d.Stereotype != null && d.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_SCHEMADIAGRAM))
                    {
                        schemaDiagram = d;
                    }
                }

                
            
                jschema = SchemaManager.schemaToJsonSchema(Repository, schemaDiagram).Value;
            }
            catch (ModelValidationException ex)
            {
                return ex.errors.messages;
            }
                        
            IList<string> messages;

            if (!json.IsValid(jschema, out messages))
            {
                logger.log("Sample is not valid:");
                return messages;
            }
            else{
                logger.log("Sample is Valid!");
                return null;
            }
                
        }
       

    }
}
