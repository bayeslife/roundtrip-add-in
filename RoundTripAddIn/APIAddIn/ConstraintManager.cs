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
    public class ConstraintManager
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
        
    
        static public void parentToJObject(EA.Repository Repository, EA.Diagram diagram, JArray container, IList<int> sampleIds, EA.Element ancestor, EA.Element constraint,IList<int> visited, IList<int> relationsVisited,DiagramCache diagramCache)
        {
            IList<EA.Element> children = new List<EA.Element>();
            visited.Add(constraint.ElementID);

            EA.Element constraintClazz = diagramCache.elementIDHash[constraint.ClassifierID];
            

            JObject jsonClass = new JObject();
            jsonClass.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_GUID, constraint.ElementGUID));
            jsonClass.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_NAME, constraint.Name));
            jsonClass.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_CLASS, constraintClazz.Name));
            jsonClass.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_STEREOTYPE, constraint.Stereotype));

            ObjectManager.addTagsToJson(constraint, jsonClass);

            container.Add(jsonClass);

            JArray sources = new JArray();
            JArray targets = new JArray();
            jsonClass.Add("source", sources);
            jsonClass.Add("target", targets);

            foreach (EA.Connector con in constraint.Connectors)
            {
                if (relationsVisited.Contains(con.ConnectorID))
                    continue;

                if (!DiagramManager.isVisible(con))
                    continue;

                relationsVisited.Add(con.ConnectorID);

                Boolean isSource = true;
                EA.Element related = null;
                if (constraint.ElementID == con.ClientID)
                {
                    isSource = false;
                    related = diagramCache.elementIDHash[con.SupplierID];
                }else
                {
                    isSource = true;
                    related = diagramCache.elementIDHash[con.ClientID];
                }

                
                String relGuid = related.ElementGUID;
                String relName = related.Name;
                String relClass = "";

                if (related.ClassifierID != 0)
                {
                    EA.Element relatedClazz = diagramCache.elementIDHash[related.ClassifierID];
                    relClass = relatedClazz.Name;
                }
                
                JObject jsonCon = new JObject();
                jsonCon.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_GUID, relGuid));
                jsonCon.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_NAME, relName));
                jsonCon.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_CLASS, relClass));                
                jsonCon.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_CONNECTOR_TYPE, con.Type));
                jsonCon.Add(new JProperty(RoundTripAddInClass.CONSTRAINT_PROPERTY_CONNECTOR_GUID, con.ConnectorGUID));

                logger.log("Related " + related.Name + "-" + relClass);
                if (isSource)
                {
                    sources.Add(jsonCon);
                }else
                {
                    targets.Add(jsonCon);
                }                                
            }            
        }

        static public void parentsToJObject(EA.Repository Repository, EA.Diagram diagram, JArray container, IList<int> sampleIds, EA.Element ancestor, IList<EA.Element> parents, IList<int> visited, IList<int> relationsVisited,DiagramCache diagramCache)
        {
            logger.log("Parents :" + parents.Count);

            foreach (EA.Element parent in parents)
            {
                parentToJObject(Repository, diagram, container, sampleIds, ancestor, parent, visited,relationsVisited,diagramCache);
            }
        }

        static public Hashtable sampleToJObject(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            Hashtable result = new Hashtable();

            IList<EA.Element> clazzes = MetaDataManager.diagramClasses(Repository, diagramCache.elementsList);

            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramCache.elementsList);

            EA.Element root = MetaDataManager.findContainer(Repository, diagram, diagramCache,RoundTripAddInClass.EA_STEREOTYPE_CONSTRAINT);

            logger.log("MetaData container:" + root.Name);
            
            EA.Element rootClassifier = MetaDataManager.extractSelection(diagramCache,root);

            MetaDataManager.extractDiagramMetaData(result, root);

            logger.log("Export container:" + rootClassifier.Name);

            String prefix = (string)result[RoundTripAddInClass.PREFIX];
            

            Dictionary<int, JObject> instances = new Dictionary<int, JObject>();
            JArray container = new JArray();
            string containerName = root.Name;
            string containerClassifier = rootClassifier.Name;

            IList<int> visited = new List<int>();
            IList<EA.Element> parents = new List<EA.Element>();
            IList<int> sampleIds = new List<int>();
            IList<int> relationsVisited = new List<int>();

            foreach (EA.Element sample in samples)
            {
                sampleIds.Add(sample.ElementID);

                if (sample.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_CONSTRAINT)
                    continue;

                if (sample.ClassfierID != root.ClassfierID)
                    //skip root elements that are the population elements.
                    continue;

                visited.Add(sample.ElementID);
                parents.Add(sample);

            }

            parentsToJObject(Repository, diagram, container, sampleIds, null, parents, visited,relationsVisited,diagramCache);

            string msg = prefix + JsonConvert.SerializeObject(container, Newtonsoft.Json.Formatting.Indented) + "\n";

            result.Add("sample", containerName);
            result.Add("class", containerClassifier);
            result.Add("json", msg);
            //result.Add("export", root.Name);
            return result;
        }

        static public void exportConstraint(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            try
            {
                DiagramManager.captureDiagramLinks(diagram);

                RepositoryHelper.createDiagramCache(Repository, diagram,diagramCache);                

                if (!diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_CONSTRAINTDIAGRAM))
                {
                    logger.log("Ignore diagram that isnt a constraint diagram");
                    return;
                }

                Hashtable ht = sampleToJObject(Repository, diagram,diagramCache);
                string sample = (string)ht["sample"];
                string clazz = (string)ht["class"];
                string container = (string)ht["json"];
                string export = (string)ht[RoundTripAddInClass.FILENAME];
                string sourcecontrolPackage = (string)ht[RoundTripAddInClass.PROJECT];

                logger.log("Project:" + sourcecontrolPackage);               

                if (container == null)
                {
                    MessageBox.Show("No object linked to root with classification sample declared nor  (older style) object of classification Request declared");
                    return;
                }
                
                EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);
               
                
                if (fileManager != null)
                {
                    fileManager.initializeAPI(sourcecontrolPackage);
                    fileManager.setDataName(RoundTripAddInClass.CONSTRAINT_PATH);
                    fileManager.setup(RoundTripAddInClass.RAML_0_8);
                    fileManager.exportData(sample, clazz, container, RoundTripAddInClass.CONSTRAINT_PATH, export);
                }
            }
            catch (ModelValidationException ex)
            {
                MessageBox.Show(ex.errors.messages.ElementAt(0).ToString());
            }
        }

       

        public static void syncConstraint(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            logger.log("Sync Constraints");
            RepositoryHelper.createDiagramCache(Repository, diagram,diagramCache);
            IList<EA.Element> diagramElements = diagramCache.elementsList;
            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramElements);

            EA.Element container = container = MetaDataManager.findContainer(Repository, diagram, diagramCache,RoundTripAddInClass.EA_STEREOTYPE_CONSTRAINT);
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
                fileManager.setDataName(RoundTripAddInClass.CONSTRAINT_PATH);
                fileManager.setup(RoundTripAddInClass.RAML_0_8);
                if (!fileManager.populationExists(container.Name, containerClassifier, RoundTripAddInClass.CONSTRAINT_PATH, container.Name))
                {
                    MessageBox.Show("No file exists at:" + fileManager.exportPath(container.Name, containerClassifier, RoundTripAddInClass.CONSTRAINT_PATH, container.Name));
                    return;
                }
                else
                {
                    string fullpath = fileManager.exportPath(containerName, containerClassifier, RoundTripAddInClass.CONSTRAINT_PATH, container.Name);
                    JArray jo = JArray.Parse(File.ReadAllText(fullpath));
                    sync_constraint(Repository, diagram,container, containerClassifierEl, jo, samplePkg,diagramCache);
                    samplePkg.Update();
                }
            }
        }


        private static EA.Element reifyElement(EA.Repository Repository,DiagramCache diagramCache,JObject jo,EA.Package pkg)
        {
            
            EA.Element element = null;
            JToken guidToken = null;

            String name = "";
            JToken nameToken = null;
            if(jo.TryGetValue(RoundTripAddInClass.CONSTRAINT_PROPERTY_NAME, out nameToken))
            {
                name = nameToken.ToString();
            }
            
                if (jo.TryGetValue(RoundTripAddInClass.CONSTRAINT_PROPERTY_GUID, out guidToken))
            {
                String guid = guidToken.ToString();
                EA.Element el = null;
                if (diagramCache.elementGuidHash.ContainsKey(guid))
                {
                    element = diagramCache.elementGuidHash[guid];
                }
                else
                {
                    element = Repository.GetElementByGuid(guid);
                }
            }

            if (element == null)
            {
                logger.log("Create Element");
                //Need to create constraint
                element = pkg.Elements.AddNew(name, "Object");
                element.Update();
                diagramCache.addElement(element);
            }
            return element;
        }


        private static void sync_constraint(EA.Repository Repository, EA.Diagram diagram, EA.Element sample, EA.Element classifier, JArray ja, EA.Package pkg, DiagramCache diagramCache)
        {
            //logger.log("Syncing Constraints from JArray:" + sample.Name);
            Dictionary<string, RunState> rs = ObjectManager.parseRunState(sample.RunState);
            Dictionary<string, RunState> nrs = new Dictionary<string, RunState>();

            foreach (JObject jo in ja.Children<JObject>())
            {
                //logger.log("Syncing Constraint:");


                EA.Element constraint = reifyElement(Repository,diagramCache,jo,pkg);

                JArray sources = (JArray)jo.GetValue("source");  
                foreach(JObject relatedJo in sources)
                {
                    EA.Element related = reifyElement(Repository, diagramCache, relatedJo,pkg);
                    JToken connectorProperty = null;
                    String connectorId = null;
                    if (relatedJo.TryGetValue(RoundTripAddInClass.CONSTRAINT_PROPERTY_CONNECTOR_GUID, out connectorProperty))
                    {
                        connectorId = connectorProperty.ToString();
                        
                    }
                    sync_relationship(Repository, diagram, connectorId,related, constraint, relatedJo, pkg);                    
                }
                JArray targets = (JArray)jo.GetValue("target");
                foreach (JObject relatedJo in targets)
                {
                    EA.Element related = reifyElement(Repository, diagramCache, relatedJo,pkg);                    
                    JToken connectorProperty = null;
                    String connectorId = null;
                    if (relatedJo.TryGetValue(RoundTripAddInClass.CONSTRAINT_PROPERTY_CONNECTOR_GUID, out connectorProperty))
                    {
                        connectorId = connectorProperty.ToString();

                    }
                    sync_relationship(Repository, diagram, connectorId, constraint,related, relatedJo, pkg);
                }
            }
        }

        private static void sync_relationship(EA.Repository Repository, EA.Diagram diagram, String connectorGuid, EA.Element source, EA.Element target, JObject jo, EA.Package pkg)
        {            
            if (checkRelationship(Repository, connectorGuid, source, target))
            {
                //logger.log("Relationship exists:"+source.Name+":"+target.Name);
            }
            else
            {
                logger.log("Adding Relationship:" + source.Name + ":" + target.Name);

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

        private static bool checkRelationship(EA.Repository repository,String connectorGuid,EA.Element source, EA.Element target)
        {
            if (repository.GetConnectorByGuid(connectorGuid)!=null)
            {
                return true;
            }
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

       
    }
}
