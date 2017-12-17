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
    public class MappingManager
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
   
        static public void parentToJObject(EA.Repository Repository, EA.Diagram diagram, JArray container, IList<int> sampleIds, EA.Element ancestor, EA.Element parent, IList<int> visited, IList<int> relationsVisited,DiagramCache diagramCache,string intertype)
        {
            IList<EA.Element> children = new List<EA.Element>();
            visited.Add(parent.ElementID);
            foreach (EA.Connector con in parent.Connectors)
            {
                
                if (relationsVisited.Contains(con.ConnectorID))
                    continue;

                if (!DiagramManager.isVisible(con))
                    continue;

                relationsVisited.Add(con.ConnectorID);

                EA.Element source = parent;                                
                EA.Element target = diagramCache.elementIDHash[con.SupplierID];

                if (source.ClassifierID != target.ClassifierID)
                {
                    if (!sampleIds.Contains(target.ElementID))
                        continue;
                }

                if (target.ElementID == parent.ElementID)
                {                    
                    target = diagramCache.elementIDHash[con.ClientID];
                }

                
                if (source.ClassifierID == target.ClassifierID)
                {
                    //If they are of the same type we maintain link direction
                    if(source.ElementID != con.ClientID)
                    {                        
                        EA.Element h = source;
                        source = target;
                        target = h;
                    }
                }                             

                String sourceGuid = source.ElementGUID;
                String sourceName = source.Name;
                String sourceClass = "";
                EA.Element sourceClazz = null;
                if (source.ClassifierID != 0)
                {
                    sourceClass = source.ClassifierName;
                    sourceClazz = diagramCache.elementIDHash[source.ClassifierID];
                    sourceClass = sourceClazz.Name;
                }                                 
                
                String targetGuid = target.ElementGUID;
                String targetName = target.Name;
                String targetClass = "";
                EA.Element targetClazz = null;
                if (target.ClassifierID != 0)
                {
                    targetClazz = diagramCache.elementIDHash[target.ClassifierID];
                    targetClass = targetClazz.Name;                    
                }

                
                if ((!intertype.Equals("Y")) && targetClass.Equals(sourceClass))
                {
                    continue;//skip inter type links
                }
                                    
                                           
                //if (visited.Contains(related.ElementID))
                 //   continue;

                JObject jsonClass = new JObject();
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_NAME, con.Name));
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_NOTES, con.Notes));
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_SOURCE, sourceGuid));                
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_SOURCE_NAME, sourceName));                
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_SOURCE_CLASS, sourceClass));                
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_TARGET, targetGuid));                
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_TARGET_NAME, targetName));                
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_TARGET_CLASS, targetClass));
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_TARGET_VALUE, "1"));
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_TYPE, con.Type));
                jsonClass.Add(new JProperty(RoundTripAddInClass.MAPPING_PROPERTY_STEREOTYPE, con.Stereotype));

                logger.log("Source " + source.Name + "-" + sourceClass + " Target " + target.Name + "-" + targetClass);
                container.Add(jsonClass);
                

                children.Add(target);                
            }            
            //parentsToJObject(Repository, diagram, container, sampleIds, parent, children,visited);
        }

        static public void parentsToJObject(EA.Repository Repository, EA.Diagram diagram, JArray container, IList<int> sampleIds, EA.Element ancestor, IList<EA.Element> parents, IList<int> visited, IList<int> relationsVisited,DiagramCache diagramCache,string intertype)
        {
            logger.log("Parents :" + parents.Count);

            foreach (EA.Element parent in parents)
            {
                logger.log("Handling:" + parent.Name);
                parentToJObject(Repository, diagram, container, sampleIds, ancestor, parent, visited,relationsVisited,diagramCache,intertype);
            }
        }

        static public Hashtable sampleToJObject(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            logger.log("SampeToJObject");
            Hashtable result = new Hashtable();

            IList<EA.Element> clazzes = MetaDataManager.diagramClasses(Repository, diagramCache.elementsList);

            IList<EA.Element> components = MetaDataManager.diagramComponents(Repository, diagramCache.elementsList);

            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramCache.elementsList);

            samples = samples.Concat(clazzes).ToList();
            samples = samples.Concat(components).ToList();

            logger.log("Samples:" + samples.Count);

            EA.Element root = MetaDataManager.findContainer(Repository, diagram, diagramCache, RoundTripAddInClass.EA_STEREOTYPE_MAPPING);
               
            logger.log("MetaData container:" + root.Name);

            EA.Element rootClassifier = null;
            Dictionary<int, JObject> instances = new Dictionary<int, JObject>();
            JArray container = new JArray();
            string containerName = "ALL";
            string containerClassifier = "Class";

            if (root.ClassifierID != 0)
            {
                rootClassifier = diagramCache.elementIDHash[root.ClassifierID];
                containerName = root.Name;
                containerClassifier = rootClassifier.Name;
                logger.log("Export container:" + rootClassifier.Name);
            }
        
            MetaDataManager.extractDiagramMetaData(result, root);

                    
            String prefix = (string)result[RoundTripAddInClass.PREFIX];
            String intertype = (string)result[RoundTripAddInClass.INCLUDE_INTERTYPE];


            IList<int> visited = new List<int>();
            IList<EA.Element> parents = new List<EA.Element>();
            IList<int> sampleIds = new List<int>();
            IList<int> relationsVisited = new List<int>();

            foreach (EA.Element sample in samples)
            {
                sampleIds.Add(sample.ElementID);

                if (sample.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_HIERARCHY)
                    continue;

                if (root.ClassifierID !=0 && sample.ClassfierID != root.ClassfierID)
                    continue;

               
                visited.Add(sample.ElementID);
                parents.Add(sample);

            }

            parentsToJObject(Repository, diagram, container, sampleIds, null, parents, visited,relationsVisited,diagramCache,intertype);

            string msg = prefix + JsonConvert.SerializeObject(container, Newtonsoft.Json.Formatting.Indented) + "\n";

            result.Add("sample", containerName);
            result.Add("class", containerClassifier);
            result.Add("json", msg);
            //result.Add("export", root.Name);
            return result;
        }

        static public void exportMapping(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            try
            {
              
                DiagramManager.captureDiagramLinks(diagram);

                //logger.log("links captured");
                RepositoryHelper.createDiagramCache(Repository, diagram,diagramCache);
                //logger.log("cache created");
                if (!diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_MAPPINGDIAGRAM))
                {
                    logger.log("exportSample: Ignore diagram that isnt a mapping diagram");
                    return;
                }
            
                Hashtable ht = sampleToJObject(Repository, diagram,diagramCache);
                string sample = (string)ht["sample"];
                string clazz = (string)ht["class"];
                string container = (string)ht["json"];
                string export = (string)ht[RoundTripAddInClass.FILENAME];
                string sourcecontrolPackage = (string)ht[RoundTripAddInClass.PROJECT];

                logger.log("Project:" + sourcecontrolPackage);
                //KeyValuePair<string,JObject> kv = sampleToJObject(Repository, diagram);
                //JObject container = kv.Value;

                if (container == null)
                {
                    MessageBox.Show("No object linked to root with classification sample declared nor  (older style) object of classification Request declared");
                    return;
                }

                //string msg = JsonConvert.SerializeObject(container, Newtonsoft.Json.Formatting.Indented) + "\n";
                EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);
               
                

                if (fileManager != null)
                {
                    fileManager.initializeAPI(sourcecontrolPackage);
                    fileManager.setDataName(RoundTripAddInClass.MAPPING_PATH);
                    fileManager.setup(RoundTripAddInClass.RAML_0_8);
                    fileManager.exportData(sample, clazz, container, RoundTripAddInClass.MAPPING_PATH, export);
                }
            }
            catch (ModelValidationException ex)
            {
                MessageBox.Show(ex.errors.messages.ElementAt(0).ToString());
            }
        }

        ///
        /// Validate all object run state keys correspond to classifier attributes
        ///
        //static public void validateDiagram(EA.Repository Repository, EA.Diagram diagram)
        //{
        //    IList<string> messages = diagramValidation(Repository, diagram);

        //    logger.log("**ValidationResults**");
        //    if (messages != null)
        //    {
        //        foreach (string m in messages)
        //        {
        //            logger.log(m);
        //        }
        //    }
        //}

        //static public IList<string> diagramValidation(EA.Repository Repository, EA.Diagram diagram)
        //{
        //    JSchema jschema = null;
        //    JObject json = null;
        //    try
        //    {
        //        //logger.log("Validate Sample");
        //        json = (JObject)sampleToJObject(Repository, diagram)["json"];

        //        //logger.log("JObject formed");

        //        EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);
        //        EA.Package samplesPackage = Repository.GetPackageByID(samplePkg.ParentID);
        //        EA.Package apiPackage = Repository.GetPackageByID(samplesPackage.ParentID);

        //        EA.Package schemaPackage = null;

        //        foreach (EA.Package p in apiPackage.Packages)
        //        {
        //            if (p != null && p.Name.Equals(RoundTripAddInClass.API_PACKAGE_SCHEMAS))
        //            {
        //                schemaPackage = p;
        //            }
        //        }
        //        if (schemaPackage == null)
        //        {
        //            throw new Exception("No Schema package found");
        //        }

        //        EA.Diagram schemaDiagram = null;
        //        foreach (EA.Diagram d in schemaPackage.Diagrams)
        //        {
        //            if (d.Stereotype != null && d.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_SCHEMADIAGRAM))
        //            {
        //                schemaDiagram = d;
        //            }
        //        }



        //        jschema = SchemaManager.schemaToJsonSchema(Repository, schemaDiagram).Value;
        //    }
        //    catch (ModelValidationException ex)
        //    {
        //        return ex.errors.messages;
        //    }

        //    IList<string> messages;

        //    if (!json.IsValid(jschema, out messages))
        //    {
        //        logger.log("Sample is not valid:");
        //        return messages;
        //    }
        //    else
        //    {
        //        logger.log("Sample is Valid!");
        //        return null;
        //    }

        //}


        public static void syncMapping(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            logger.log("Sync Mapping");
            RepositoryHelper.createDiagramCache(Repository, diagram,diagramCache);
            IList<EA.Element> diagramElements = diagramCache.elementsList;
            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramElements);

            EA.Element container = container = MetaDataManager.findContainer(Repository, diagram, diagramCache,RoundTripAddInClass.EA_STEREOTYPE_MAPPING);
            EA.Element containerClassifierEl = diagramCache.elementIDHash[container.ClassfierID];
            string containerName = container.Name;
            string containerClassifier = containerClassifierEl.Name;

            EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);


            Hashtable ht = new Hashtable();
            MetaDataManager.extractDiagramMetaData(ht, container);

            string project = (String)ht[RoundTripAddInClass.PROJECT];

            
            if (fileManager != null)
            {
                fileManager.initializeAPI(project);
                fileManager.setDataName(RoundTripAddInClass.MAPPING_PATH);
                fileManager.setup(RoundTripAddInClass.RAML_0_8);
                if (!fileManager.populationExists(container.Name, containerClassifier, RoundTripAddInClass.MAPPING_PATH, container.Name))
                {
                    MessageBox.Show("No file exists at:" + fileManager.exportPath(container.Name, containerClassifier, RoundTripAddInClass.MAPPING_PATH, container.Name));
                    return;
                }
                else
                {
                    string fullpath = fileManager.exportPath(containerName, containerClassifier, RoundTripAddInClass.MAPPING_PATH, container.Name);
                    JArray jo = JArray.Parse(File.ReadAllText(fullpath));
                    sync_mapping(Repository, diagram,container, containerClassifierEl, jo, samplePkg,diagramCache);
                    samplePkg.Update();
                }
            }
        }


        private static void sync_mapping(EA.Repository Repository, EA.Diagram diagram,EA.Element sample, EA.Element classifier, JArray ja, EA.Package pkg,DiagramCache diagramCache)
        {
            logger.log("Syncing JArray:" + sample.Name);
            Dictionary<string, RunState> rs = ObjectManager.parseRunState(sample.RunState);
            Dictionary<string, RunState> nrs = new Dictionary<string, RunState>();

            foreach (JObject jo in ja.Children<JObject>())
            {
                logger.log("Syncing Relationship:");

                EA.Element source = null;
                EA.Element target = null;

                JToken guidToken = null;
                if (jo.TryGetValue(RoundTripAddInClass.MAPPING_PROPERTY_SOURCE, out guidToken))
                {
                    String guid = guidToken.ToString();
                    EA.Element el = Repository.GetElementByGuid(guid);
                    if (el != null)
                    {
                        //logger.log("Found element for guid" + guid);                        
                        source = el;
                    }
                    else
                    {
                        logger.log("No element for guid" + guid);
                    }

                }
                if (jo.TryGetValue(RoundTripAddInClass.MAPPING_PROPERTY_TARGET, out guidToken))
                {
                    String guid = guidToken.ToString();
                    EA.Element el = Repository.GetElementByGuid(guid);
                    if (el != null)
                    {
                        //logger.log("Found element for guid" + guid);                        
                        target = el;
                    }
                    else
                    {
                        logger.log("No element for guid" + guid);
                    }

                }
                if (source != null && target != null)
                {
                   if(!checkRelationship(source,target))
                        sync_relationship(Repository, diagram, source, target, jo, pkg);
                }
                                                                
            }
        }


        private static bool checkRelationship(EA.Element source, EA.Element target)
        {
            foreach (EA.Connector con in source.Connectors)
            {
                if (source.ElementID == con.ClientID && target.ElementID == con.SupplierID)
                {
                    //relationship already exists
                    logger.log("Relationship already exists:" + con.Name);
                    return true;
                }
                else if (source.ElementID == con.SupplierID && target.ElementID == con.ClientID)
                {
                    //relationship already exists
                    logger.log("Relationship already exists:" + con.Name);
                    return true;
                }
            }
            foreach (EA.Connector con in target.Connectors)
            {
                if (source.ElementID == con.ClientID && target.ElementID == con.SupplierID)
                {
                    //relationship already exists
                    logger.log("Relationship already exists:" + con.Name);
                    return true;
                }
                else if (source.ElementID == con.SupplierID && target.ElementID == con.ClientID)
                {
                    //relationship already exists
                    logger.log("Relationship already exists:" + con.Name);
                    return true;
                }
            }
            return false;
        }

        private static void sync_relationship(EA.Repository Repository, EA.Diagram diagram, EA.Element source, EA.Element target, JObject jo, EA.Package pkg)
        {
            logger.log("Adding Relationship:");              
                                 
                    //Add relationship  logger.log("No  parent link found");            
                    EA.Connector link = source.Connectors.AddNew("", "Association");
                    link.SupplierID = target.ElementID;
                    link.Update();
                    source.Update();
                    target.Update();

                    EA.DiagramLink dl = diagram.DiagramLinks.AddNew("", "");
                    dl.ConnectorID = link.ConnectorID;
                    dl.Update();

                    source.Connectors.Refresh();
                    target.Connectors.Refresh();                            
        }
    }
}
