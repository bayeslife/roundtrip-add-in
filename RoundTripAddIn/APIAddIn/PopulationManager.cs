﻿using Newtonsoft.Json;
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

       
        public static void syncPopulation(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            logger.log("Sync Population");
            RepositoryHelper.createDiagramCache(Repository, diagram,diagramCache);
            IList<EA.Element> diagramElements = diagramCache.elementsList;

            //IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramElements);
            IList<EA.Element> samples = diagramElements;

            EA.Element container = container = MetaDataManager.findContainer(Repository, diagram, diagramCache, RoundTripAddInClass.EA_STEREOTYPE_POPULATION);

            logger.log("Classifier ID:" + container.ClassifierID);


            string containerName = container.Name;
            string containerClassifier = "Classes";
            EA.Element containerClassifierEl = null;
            if (container.ClassifierID!=0)
            {
                containerClassifierEl = Repository.GetElementByID(container.ClassfierID);
                containerName = container.Name;
                containerClassifier = containerClassifierEl.Name;
            }

            EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);

            Hashtable ht = new Hashtable();
            MetaDataManager.extractDiagramMetaData(ht, container);

            string project = (String)ht[RoundTripAddInClass.PROJECT];

            if (project == null)
            {
                MessageBox.Show("No project defined in the Population stereotyped element. Please enter project name where population should be exported.");
                return;
            }

            if (fileManager != null) {
                fileManager.initializeAPI(project);
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
                    sync_population(Repository, container, containerClassifierEl, jo, samplePkg,diagramCache);
                    samplePkg.Update();
                }
            }
        }


        private static void sync_population(EA.Repository Repository, EA.Element sample, EA.Element classifier, JArray ja, EA.Package pkg,DiagramCache diagramCache)
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
                    EA.Element el = diagramCache.elementGuidHash[guid];
                    if(el==null)
                        el = Repository.GetElementByGuid(guid);
                    if (el != null)
                    {
                        //logger.log("Found element for guid" + guid);
                        ObjectManager.sync_element_taggedvalue(Repository, el, classifier, jo, pkg,diagramCache);
                    } else
                    {
                        logger.log("No element for guid" + guid);
                    }
                } else
                {
                    logger.log("No guid, adding element" + jo.ToString());
                    EA.Element el = pkg.Elements.AddNew("", "Object");
                    logger.log("No guid, adding element" + jo.ToString());
                    ObjectManager.sync_element_taggedvalue(Repository, el, classifier, jo, pkg,diagramCache);

                }
            }
        }


        private static void sync_population_runstate(EA.Repository Repository, EA.Element sample, EA.Element classifier, JObject jo,EA.Package pkg,DiagramCache diagramCache)
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
        }


        static public Hashtable sampleToJObject(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramElements)
        {
            Hashtable result = new Hashtable();

            //logger.log("sampleToObject");

            //IList<EA.Element> clazzes = MetaDataManager.diagramClasses(Repository, diagramElements.elementsList);
            //logger.log("GetClazzes" + clazzes.Count);

            //IList<EA.Element> components = MetaDataManager.diagramComponents(Repository, diagramElements.elementsList);
            //logger.log("GetComponents" + components.Count);


            //IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramElements.elementsList);
            //logger.log("GetSamples" + samples.Count);

            //samples = samples.Concat(clazzes).ToList();
            //samples = samples.Concat(components).ToList();
            IList<EA.Element> samples = diagramElements.elementsList;
            logger.log("All" + samples.Count);

            EA.Element root = MetaDataManager.findContainer(Repository, diagram, diagramElements, RoundTripAddInClass.EA_STEREOTYPE_POPULATION);

            MetaDataManager.extractDiagramMetaData(result, root);

            logger.log("Classifier ID:" + root.ClassifierID);

            Dictionary<int, JObject> instances = new Dictionary<int, JObject>();
            JArray container = new JArray();
            string containerName = root.Name;
            string containerClassifier = "Classes";

            if (root.ClassifierID != 0)
            {                   
                EA.Element rootClassifier = Repository.GetElementByID(root.ClassifierID);                                
                containerName = root.Name;
                containerClassifier = rootClassifier.Name;
            }                       

            foreach (EA.Element sample in samples)
            {
                //logger.log("Sample Name:" + sample.Name);

                if (sample.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_POPULATION)
                {
                    continue;
                }
                    
                if (root.ClassifierID!=0 && sample.ClassfierID != root.ClassfierID)
                    //skip root elements that are the population elements.
                    continue;

                //logger.log("Sample Name2:" + sample.Name);

                String type = "";
                EA.Element clazz = null;
                if (sample.ClassifierID != 0)
                {
                    clazz = diagramElements.elementIDHash[sample.ClassifierID];
                    type = clazz.Name;
                } else {
                    logger.log("Classifier is null");
                }
                                                                
                EA.Package package = diagramElements.packageIDHash[sample.PackageID];

                JObject jsonClass = null;

                {
                    jsonClass = new JObject();                    
                    jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_GUID, sample.ElementGUID));
                    jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_NAME, sample.Name));
                    jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_NOTES, sample.Notes));
                    jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_PACKAGE, package.Name));
                    if (clazz!=null)
                        jsonClass.Add(new JProperty(RoundTripAddInClass.POPULATION_PROPERTY_TYPE, clazz.Name));

                    container.Add(jsonClass);
                }

                string rs = sample.RunState;
                
                ObjectManager.addRunStateToJson(rs, jsonClass);
                ObjectManager.addTagsToJson(sample, jsonClass);
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

        static public void exportPopulation(EA.Repository Repository, EA.Diagram diagram,DiagramCache diagramCache)
        {
            try
            {
                if (!diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_POPULATIONDIAGRAM))
                {
                    logger.log("exportSample: Ignore diagam that isnt a population diagram");
                    return;
                }

                RepositoryHelper.createDiagramCache(Repository, diagram,diagramCache);
                IList<EA.Element> diagramElements = diagramCache.elementsList;

                Hashtable ht = sampleToJObject(Repository, diagram, diagramCache);
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
                //EA.Package samplePkg = Repository.GetPackageByID(diagram.PackageID);
                //EA.Package samplesPackage = Repository.GetPackageByID(samplePkg.ParentID);
                //EA.Package apiPackage = Repository.GetPackageByID(samplesPackage.ParentID);

                
                String project = (String)ht[RoundTripAddInClass.PROJECT];
                if(project==null)
                    project = RoundTripAddInClass.EXPORT_PACKAGE;


                if (fileManager != null)
                {
                    fileManager.initializeAPI(project);
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
        //        json = (JObject)sampleToJObject(Repository, diagram)["json"];

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
       

    }
}
