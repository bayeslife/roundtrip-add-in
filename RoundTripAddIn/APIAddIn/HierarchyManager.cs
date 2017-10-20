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

        static EA.Element findContainer(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            logger.log("Finding container for diagram:" + diagram.Name);

            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramCache.elementsList);
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

        static public void parentToJObject(EA.Repository Repository, EA.Diagram diagram, JArray container, IList<int> sampleIds, EA.Element ancestor,EA.Element parent, IList<int> visited,int depth,DiagramCache diagramCache)
        {
            String type = "";
            if (parent.ClassifierID != 0)
            {
                //logger.log("Get Parent Classifier");
                EA.Element classifier = diagramCache.elementIDHash[parent.ClassifierID];
                type = classifier.Name;
            }                

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
            
            ObjectManager.addTagsToJson(parent, jsonClass);
            ObjectManager.addRunStateToJson(parent.RunState, jsonClass);

            IList<EA.Element> children = new List<EA.Element>();
            visited.Add(parent.ElementID);
            foreach (EA.Connector con in parent.Connectors)
            {
                if (!DiagramManager.isVisible(con)) //skip not visiable
                    continue;

                EA.Element related = DiagramManager.getVisibleRelatedElement(Repository,parent, con, diagramCache);
                if (related == null)
                    continue;
                    
                //logger.log("Parent" + parent.Name);
                //logger.log("Related"+ related.Name);
                if (!sampleIds.Contains(related.ElementID))
                    continue;

                if (visited.Contains(related.ElementID))
                    continue;
                
                children.Add(related);

                logger.log("Parent:" + parent.Name + " Child:"+related.Name);
            }

            parentsToJObject(Repository, diagram, container, sampleIds, parent, children,visited,++depth,diagramCache);
         }

        static public void parentsToJObject(EA.Repository Repository, EA.Diagram diagram, JArray container,IList<int> sampleIds,EA.Element ancestor,IList<EA.Element> parents,IList<int> visited,int depth,DiagramCache diagramCache)
        {
            //logger.log("Parents :" + parents.Count);            
            foreach (EA.Element parent in parents)
            {
                parentToJObject(Repository, diagram, container, sampleIds, ancestor,parent, visited,depth,diagramCache);
            }                        
        }

        static public Hashtable sampleToJObject(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            
            Hashtable result = new Hashtable();
       
            //IList<EA.Element> samples = MetaDataManager.diagramElements(Repository, diagram);
            
            logger.log("Elements size:" + diagramCache.elementsList.Count);

            //DiagramElements diagramCache = RepositoryHelper.getDiagramElements(Repository, diagram.DiagramObjects);
            //IList<EA.Element> diagramElements = diagramCache.elementsList;

            EA.Element root = findContainer(Repository, diagram, diagramCache);            

            MetaDataManager.extractDiagramMetaData(result, root);

            int level =(int) result[RoundTripAddInClass.HIERARCHY_LEVEL];
            //String prefix = "";
            //String filename = "";  
            
            EA.Element rootClassifier = diagramCache.elementIDHash[root.ClassifierID];

            logger.log("Export container:" + rootClassifier.Name );

            Dictionary<int, JObject> instances = new Dictionary<int, JObject>();
            JArray container = new JArray();
            string containerName = root.Name;
            string containerClassifier = rootClassifier.Name;
           
            IList<int> visited = new List<int>();
            IList<EA.Element> parents = new List<EA.Element>();
            IList<int> sampleIds = new List<int>();

            foreach (EA.Element sample in diagramCache.elementsList)
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

            parentsToJObject(Repository, diagram, container,sampleIds, null,parents, visited,level,diagramCache);


            string msg = result[RoundTripAddInClass.PREFIX] + JsonConvert.SerializeObject(container, Newtonsoft.Json.Formatting.Indented) + "\n";

            String filename = (String)result[RoundTripAddInClass.FILENAME];
            if (filename.Length ==0)
            {
                filename = root.Name;
                result.Remove(RoundTripAddInClass.FILENAME);
                result.Add(RoundTripAddInClass.FILENAME, filename);                
            }

            result.Add("sample", containerName);
            result.Add("class", containerClassifier);
            result.Add("json", msg);                        
            
            return result;
        }

        static public void exportHierarchy(EA.Repository Repository, EA.Diagram diagram)
        {              
            try
            {
                if (!diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_HIERARCHYDIAGRAM))
                {
                    logger.log("exportSample: Ignore diagram that isnt a hierarchy diagram");
                    return;
                }

                DiagramManager.captureDiagramLinks(diagram);

                DiagramCache diagramCache = RepositoryHelper.createDiagramCache(Repository, diagram);                
                Hashtable ht = sampleToJObject(Repository, diagram,diagramCache);
                
                string sample = (string)ht["sample"];
                string clazz = (string)ht["class"];
                string container = (string)ht["json"];
                string exportName = (string)ht[RoundTripAddInClass.FILENAME];   
                string projectName = (string)ht[RoundTripAddInClass.PROJECT];


                if (container == null)
                {
                    MessageBox.Show("No object linked to root with classification sample declared nor  (older style) object of classification Request declared");
                    return;
                }
                       
                if(projectName==null)
                    projectName = RoundTripAddInClass.EXPORT_PACKAGE;

                logger.log("saving");
                if (fileManager != null)
                {
                    fileManager.initializeAPI(projectName);
                    fileManager.setDataName(RoundTripAddInClass.HIERARCHY_PATH);
                    fileManager.setup(RoundTripAddInClass.RAML_0_8);                    
                    fileManager.exportData(sample, clazz, container,RoundTripAddInClass.HIERARCHY_PATH,exportName);
                }           
          }catch(ModelValidationException ex){
            MessageBox.Show(ex.errors.messages.ElementAt(0).ToString());
          }            
        }        

        ///
        /// Validate all object run state keys correspond to classifier attributes
        ///
        //static public void validateDiagram(EA.Repository Repository,EA.Diagram diagram)
        //{                        
        //    IList<string> messages = diagramValidation(Repository,diagram);

        //    logger.log("**ValidationResults**");
        //    if(messages!=null)
        //    {                
        //        foreach (string m in messages)
        //        {
        //            logger.log(m);                    
        //        }                                
        //    }                        
        //}

        //static public IList<string> diagramValidation(EA.Repository Repository, EA.Diagram diagram,IList<EA.Element> diagramElements)
        //{
        //    JSchema jschema = null;
        //    JObject json = null;
        //    try
        //    {
        //        //logger.log("Validate Sample");
        //        json = (JObject)sampleToJObject(Repository, diagram, diagramElements)["json"];

        //        //logger.log("JObject formed");
            
        //        EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);            
        //        EA.Package samplesPackage = Repository.GetPackageByID(samplePkg.ParentID);            
        //        EA.Package apiPackage = Repository.GetPackageByID(samplesPackage.ParentID);
            
        //        EA.Package schemaPackage = null;
            
        //        foreach (EA.Package p in apiPackage.Packages)
        //        {                
        //            if (p!=null && p.Name.Equals(RoundTripAddInClass.API_PACKAGE_SCHEMAS))
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

                
            
        //        jschema = SchemaManager.schemaToJsonSchema(Repository, schemaDiagram,diagramElements).Value;
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
        //    else{
        //        logger.log("Sample is Valid!");
        //        return null;
        //    }
                
        //}


        public static void syncHierarchy(EA.Repository Repository, EA.Diagram diagram)
        {
            logger.log("Sync Hierarchy");

            DiagramCache diagramCache = RepositoryHelper.createDiagramCache(Repository, diagram);
            //IList<EA.Element> diagramElements = diagramCache.elementsList;

            IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramCache.elementsList);

            EA.Element container = container = findContainer(Repository, diagram, diagramCache);
            EA.Element containerClassifierEl = diagramCache.elementIDHash[container.ClassfierID];
            string containerName = container.Name;
            string containerClassifier = containerClassifierEl.Name;

            Hashtable ht = new Hashtable();
            MetaDataManager.extractDiagramMetaData(ht, container);

            EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);

            string sourcecontrolPackage = (string)ht[RoundTripAddInClass.PROJECT];


            if (fileManager != null)
            {
                fileManager.initializeAPI(sourcecontrolPackage);
                fileManager.setDataName(RoundTripAddInClass.HIERARCHY_PATH);
                fileManager.setup(RoundTripAddInClass.RAML_0_8);
                if (!fileManager.populationExists(container.Name, containerClassifier, RoundTripAddInClass.HIERARCHY_PATH, container.Name))
                {
                    MessageBox.Show("No file exists at:" + fileManager.exportPath(container.Name, containerClassifier, RoundTripAddInClass.HIERARCHY_PATH, container.Name));
                    return;
                }
                else
                {
                    string fullpath = fileManager.exportPath(containerName, containerClassifier, RoundTripAddInClass.HIERARCHY_PATH, container.Name);
                    JArray jo = JArray.Parse(File.ReadAllText(fullpath));
                    sync_hierarchy(Repository, diagram,container, jo, samplePkg,diagramCache);
                    samplePkg.Update();
                    diagram.DiagramLinks.Refresh();
                    if (!diagram.Update())
                    {
                        logger.log(diagram.GetLastError());
                    }
                    
                }
            }
        }

        private static void sync_hierarchy(EA.Repository Repository, EA.Diagram diagram,EA.Element sample, JArray ja, EA.Package pkg,DiagramCache diagramCache)
        {
            logger.log("Syncing JArray:" + sample.Name);
            Dictionary<string, RunState> rs = ObjectManager.parseRunState(sample.RunState);
            Dictionary<string, RunState> nrs = new Dictionary<string, RunState>();

            foreach (JObject jo in ja.Children<JObject>())
            {
                logger.log("Syncing Child:");
                JToken guidToken = null;
                if (jo.TryGetValue(RoundTripAddInClass.HIERARCHY_PROPERTY_ID, out guidToken))
                {                                     

                    String guid = guidToken.ToString();
                    EA.Element el = diagramCache.elementGuidHash[guid];
                    if(el==null)
                        el = Repository.GetElementByGuid(guid);

                    if (el != null)
                    {
                        //logger.log("Found element for guid" + guid);
                        sync_hierarchy(Repository, diagram,el, jo, pkg,diagramCache);
                    }
                    else
                    {
                        logger.log("No element for id" + guid);
                    }
                }
                else
                {
                    logger.log("No id, adding element" + jo.ToString());
                    EA.Element el = pkg.Elements.AddNew("", "Object");
                    logger.log("No guid, adding element" + jo.ToString());
                    sync_hierarchy(Repository, diagram, el, jo, pkg,diagramCache);

                }
            }
        }

        private static void sync_hierarchy(EA.Repository Repository, EA.Diagram diagram,EA.Element sample, JObject jo, EA.Package pkg,DiagramCache diagramCache)
        {
            logger.log("Syncing JObject:" + sample.Name);
            Dictionary<string, RunState> rs = ObjectManager.parseRunState(sample.RunState);
            Dictionary<string, RunState> nrs = new Dictionary<string, RunState>();

            foreach (JProperty p in jo.Properties())
            {
                logger.log("Property:" + p.Name+":"+ p.Value.ToString());
                if (p.Name == RoundTripAddInClass.HIERARCHY_PROPERTY_LEVEL)
                {
                    continue;
                }
                if (p.Name == RoundTripAddInClass.HIERARCHY_PROPERTY_ID)
                {
                    continue;
                }
                if (p.Name == RoundTripAddInClass.HIERARCHY_PROPERTY_NAME)
                {
                    sample.Name = p.Value.ToString();
                    continue;
                }
                if (p.Name == RoundTripAddInClass.HIERARCHY_PROPERTY_DESCRIPTION)
                {
                    sample.Notes = p.Value.ToString();
                    continue;
                }

                if (p.Name == RoundTripAddInClass.HIERARCHY_PROPERTY_TYPE)
                {
                    string classifierName = p.Value.ToString();
                    EA.Element elementClassifier = diagramCache.elementIDHash[sample.ClassifierID];
                    if (elementClassifier == null || elementClassifier.Name != classifierName)
                    {
                        EA.Element clazz = RepositoryHelper.queryClassifier(Repository, classifierName);
                        if (clazz != null)
                        {
                            sample.ClassifierID = clazz.ElementID;
                            continue;
                        }
                    }else
                    {                        
                    }
                    continue;
                    
                }
                if (p.Name == RoundTripAddInClass.HIERARCHY_PROPERTY_PARENT)
                {
                    string guid = p.Value.ToString();
                    if (guid == null || guid.Length == 0 || guid == "null")
                        continue;

                    EA.Element parent = null;
                    if (diagramCache.elementGuidHash.ContainsKey(guid))
                    {
                        parent = diagramCache.elementGuidHash[guid];
                    }                    
                    if(parent==null)
                        parent = Repository.GetElementByGuid(guid);
                    if (parent == null)
                    {
                        logger.log("missing parent");
                        continue;
                    } else
                    {
                        linkToParent(Repository, diagram,sample, parent);
                    }
                    continue;
                }

                //string rsv=null;
                if (p.Value.Type != JTokenType.Object && p.Value.Type != JTokenType.Array)
                {
                    //logger.log("Adding Property:" + sample.Name);
                    RunState r;
                    if (rs.ContainsKey(p.Name))
                    {
                        r = rs[p.Name];
                    }
                    else
                    {
                        r = new RunState();
                        r.key = p.Name;
                    }
                    r.value = p.Value.ToString();

                    nrs.Add(r.key, r);
                }
            }

            sample.RunState = ObjectManager.renderRunState(nrs);
            logger.log(sample.RunState);
            sample.Update();

            foreach (EA.Connector con in sample.Connectors)
            {                
                EA.Element related = null;

                if (!DiagramManager.isVisible(con)) //skip not visiable
                    continue;

                //logger.log("Connector:" + con.SupplierEnd.Role);

                if (sample.ElementID == con.ClientID)
                {
                    if(diagramCache.elementIDHash.ContainsKey(con.SupplierID))
                        related = diagramCache.elementIDHash[con.SupplierID];

                    if (related==null)
                        related = Repository.GetElementByID(con.SupplierID);

                    JProperty p = jo.Property(con.SupplierEnd.Role);

                    if (p != null)
                    {
                        //logger.log("Found Json Property:" + con.SupplierEnd.Role);
                        if (p.Value.Type == JTokenType.Object)
                        {
                            JObject pjo = (JObject)p.Value;
                            sync_hierarchy(Repository, diagram, related, pjo, pkg,diagramCache);
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
                                    sync_hierarchy(Repository, diagram,related, (JObject)t, pkg,diagramCache);
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

        private static void linkToParent(EA.Repository Repository, EA.Diagram diagram,EA.Element sample, EA.Element parent)        
        {
            foreach (EA.Connector con in sample.Connectors)
            {
                logger.log("Connector:" + con.SupplierEnd.Role);
                EA.Element related = null;

                if (sample.ElementID == con.ClientID && parent.ElementID == con.SupplierID)
                {
                    logger.log("Found parent link");
                    return;
                }
                else if (sample.ElementID == con.SupplierID && parent.ElementID == con.ClientID)
                {
                    logger.log("Found parent link");
                    return;
                }
            }

            logger.log("No  parent link found");            
            EA.Connector link = sample.Connectors.AddNew("", "Association");
            link.SupplierID = parent.ElementID;
            link.Update();
            
            EA.DiagramLink dl = diagram.DiagramLinks.AddNew("", "");            
            dl.ConnectorID = link.ConnectorID;
            sample.Connectors.Refresh();
            dl.Update();
            
        }


    }
}
