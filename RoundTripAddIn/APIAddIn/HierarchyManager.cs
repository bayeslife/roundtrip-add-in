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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace RoundTripAddIn
{
    public class HierarchyManager
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
                if (sample.Stereotype != null && sample.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_HIERARCHY)
                {
                    logger.log("Hierachy is identified by Hierarchy stereotype");

                    return sample;
                }
            }
            throw new ModelValidationException("Unable to find Object stereotyped as Hierarchy on the diagram");
        }

        static public void parentToJObject(EA.Repository Repository, EA.Diagram diagram, JArray container, IList<int> sampleIds, EA.Element ancestor,EA.Element parent, IList<int> visited,int depth)
        {

            String type = "";
            if(parent.ClassifierID!=0)
                type = Repository.GetElementByID(parent.ClassifierID).Name;

            JObject jsonClass = new JObject();
            jsonClass.Add(new JProperty(RoundTripAddInClass.HIERARCHY_PROPERTY_TYPE, type));
            jsonClass.Add(new JProperty(RoundTripAddInClass.HIERARCHY_PROPERTY_ID, parent.ElementGUID));
            jsonClass.Add(new JProperty(RoundTripAddInClass.HIERARCHY_PROPERTY_NAME, parent.Name));
            jsonClass.Add(new JProperty(RoundTripAddInClass.HIERARCHY_PROPERTY_DESCRIPTION, parent.Notes));
            jsonClass.Add(new JProperty(RoundTripAddInClass.HIERARCHY_PROPERTY_LEVEL, depth));
            if (ancestor!=null)
                jsonClass.Add(new JProperty(RoundTripAddInClass.HIERARCHY_PROPERTY_PARENT, ancestor.ElementGUID));
            else
                jsonClass.Add(new JProperty(RoundTripAddInClass.HIERARCHY_PROPERTY_PARENT, "null"));
            container.Add(jsonClass);

            IList<EA.Element> children = new List<EA.Element>();
            visited.Add(parent.ElementID);
            foreach (EA.Connector con in parent.Connectors)
            {

                if (!DiagramManager.isVisible(con)) //skip not visiable
                    continue;


                    EA.Element related = Repository.GetElementByID(con.SupplierID);
                if(related.ElementID== parent.ElementID)
                    related = Repository.GetElementByID(con.ClientID);
                logger.log("Parent" + parent.Name);
                logger.log("Related"+ related.Name);
                if (!sampleIds.Contains(related.ElementID))
                    continue;

                if (visited.Contains(related.ElementID))
                    continue;
                
                children.Add(related);

                logger.log("Parent:" + parent.Name + " Child:"+related.Name);

                
                                                 
            }

            parentsToJObject(Repository, diagram, container, sampleIds, parent, children,visited,++depth);
         }

        static public void parentsToJObject(EA.Repository Repository, EA.Diagram diagram, JArray container,IList<int> sampleIds,EA.Element ancestor,IList<EA.Element> parents,IList<int> visited,int depth)
        {
            logger.log("Parents :" + parents.Count);
            
            foreach (EA.Element parent in parents)
            {
                parentToJObject(Repository, diagram, container, sampleIds, ancestor,parent, visited,depth);
            }                        
        }

        static public Hashtable sampleToJObject(EA.Repository Repository, EA.Diagram diagram)
        {
            
            Hashtable result = new Hashtable();

            IList<EA.Element> clazzes = MetaDataManager.diagramClasses(Repository, diagram);

            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagram);

            EA.Element root = findContainer(Repository, diagram);

            logger.log(root.RunState);

            int level = 1;            
            Dictionary<string, RunState> rs = ObjectManager.parseRunState(root.RunState);
            if (rs.ContainsKey(RoundTripAddInClass.HIERARCHY_LEVEL))
            {
                level = Int32.Parse(rs[RoundTripAddInClass.HIERARCHY_LEVEL].value);
            }
            logger.log("Level is:"+level);

            EA.Element rootClassifier = Repository.GetElementByID(root.ClassifierID);

            logger.log("Export container:" + rootClassifier.Name );

            Dictionary<int, JObject> instances = new Dictionary<int, JObject>();
            JArray container = new JArray();
            string containerName = root.Name;
            string containerClassifier = rootClassifier.Name;
           
            IList<int> visited = new List<int>();
            IList<EA.Element> parents = new List<EA.Element>();
            IList<int> sampleIds = new List<int>();

            foreach (EA.Element sample in samples)
            {
                sampleIds.Add(sample.ElementID);

                if (sample.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_HIERARCHY)
                    continue;

                if (sample.ClassfierID != root.ClassfierID)
                    //skip root elements that are the population elements.
                    continue;

                visited.Add(sample.ElementID);
                parents.Add(sample);

            }

            parentsToJObject(Repository, diagram, container,sampleIds, null,parents, visited,level);

            result.Add("sample", containerName);
            result.Add("class", containerClassifier);
            result.Add("json", container);
            result.Add("export", root.Name);
            return result;
        }

        static public void exportHierarchy(EA.Repository Repository, EA.Diagram diagram)
        {            
            try
            {
                if (!diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_HIERARCHYDIAGRAM))
                {
                    logger.log("exportSample: Ignore diagam that isnt a hierarchy diagram");
                    return;
                }

                DiagramManager.captureDiagramLinks(diagram);

                Hashtable ht = sampleToJObject(Repository, diagram);
                string sample = (string)ht["sample"];
                string clazz = (string)ht["class"];
                JArray container = (JArray)ht["json"];
                string exportName = (string)ht["export"];


                //logger.log("Population Size:" + container.Count);
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
                    fileManager.setDataName(RoundTripAddInClass.HIERARCHY_PATH);
                    fileManager.setup(RoundTripAddInClass.RAML_0_8);                    
                    fileManager.exportData(sample, clazz, msg,RoundTripAddInClass.HIERARCHY_PATH,exportName);
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
