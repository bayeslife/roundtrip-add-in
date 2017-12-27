using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace RoundTripAddIn
{

    public class MetaDataManager
    {

        static Profiler profiler = new Profiler();
        static Logger logger = new Logger();

        static public void setLogger(Logger l)
        {
            logger = l;
            profiler.setLogger(logger);
        }


        //public static DiagramElements diagramElements(EA.Repository Repository, EA.Diagram diagram)
        //{                       
        //    DiagramElements diagramElements = RepositoryHelper.getDiagramElements(Repository, diagram.DiagramObjects);

        //    return diagramElements;
        //}

        static public IList<EA.Element> diagramSamples(EA.Repository Repository, IList<EA.Element> elements)
        {
            List<EA.Element> samples = new List<EA.Element>();
            foreach (EA.Element el in elements)
            {
                if (el.Type.Equals(RoundTripAddInClass.EA_TYPE_OBJECT))
                    samples.Add(el);
            }
            return samples;
        }

        public static IList<EA.Element> diagramClasses(EA.Repository Repository, IList<EA.Element> elements)
        {
            List<EA.Element> samples = new List<EA.Element>();
            foreach (EA.Element el in elements)
            {
                if (el.Type.Equals(RoundTripAddInClass.EA_TYPE_CLASS) || el.Type.Equals(RoundTripAddInClass.EA_TYPE_ENUMERATION))
                    samples.Add(el);
            }
            return samples;
        }

        public static IList<EA.Element> diagramComponents(EA.Repository Repository, IList<EA.Element> elements)
        {
            List<EA.Element> samples = new List<EA.Element>();
            foreach (EA.Element el in elements)
            {
                if (el.Type.Equals(RoundTripAddInClass.EA_TYPE_COMPONENT))
                    samples.Add(el);
            }
            return samples;
        }

        


        public static IList<EA.Element> diagramElements(EA.Repository Repository)
        {
            List<EA.Element> samples = new List<EA.Element>();
            EA.Diagram diagram = null;
            if (Repository.GetContextItemType() == EA.ObjectType.otDiagram)
                diagram = Repository.GetContextObject();


            foreach (EA.DiagramObject diagramObject in diagram.DiagramObjects)
            {
                EA.Element el = Repository.GetElementByID(diagramObject.ElementID);
                samples.Add(el);
            }
            return samples;
        }


        /* Finds the Objects with a classifier of API on the diagram */
        public static EA.Element diagramAPI(EA.Repository Repository, EA.Diagram diagram)
        {
            foreach (EA.DiagramObject diagramObject in diagram.DiagramObjects)
            {
                EA.Element el = Repository.GetElementByID(diagramObject.ElementID);
                if (el.Type.Equals(RoundTripAddInClass.EA_TYPE_OBJECT))
                {
                    EA.Element classifier = Repository.GetElementByID(el.ClassifierID);
                    if (classifier.Name.Equals(RoundTripAddInClass.METAMODEL_API))
                    {
                        return el;
                    }
                }
            }
            return null;
        }


        public static void setAsSchemaDiagram(EA.Repository Repository, EA.Diagram diagram)
        {
            diagram.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_SCHEMADIAGRAM;
            diagram.Update();
        }
        public static void setAsSampleDiagram(EA.Repository Repository, EA.Diagram diagram)
        {
            diagram.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_SAMPLEDIAGRAM;
            diagram.Update();
        }
        //public static void setAsPopulationDiagram(EA.Repository Repository, EA.Diagram diagram)
        //{
        //    diagram.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_POPULATIONDIAGRAM;
        //    diagram.Update();
        //}


        public static bool filterMethod(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null && classifier.Name.Equals(RoundTripAddInClass.METAMODEL_METHOD))
                return true;
            return false;
        }
        public static bool filterQueryParameter(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null && classifier.Name.Equals(RoundTripAddInClass.METAMODEL_QUERY_PARAMETER))
                return true;
            return false;
        }

        public static bool filterPermission(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null && classifier.Name.Equals(RoundTripAddInClass.METAMODEL_PERMISSION))
                return true;
            return false;
        }

        public static bool filterResponse(EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null && classifier.Name.Equals(RoundTripAddInClass.METAMODEL_RESPONSE))
                return true;
            return false;
        }

        public static bool filterResource(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null && classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_RESOURCE))
                return true;
            return false;
        }

        public static bool filterTypeForResource(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null && classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_TYPE_FOR_RESOURCE))
                return true;
            return false;
        }

        public static bool filterResourceType(EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null && classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_RESOURCETYPE))
                return true;
            return false;
        }

        public static bool filterSecurity(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier == null)
            {
                //logger.log("Filtering for Security: Ignoring" + e.Name);
            }
            if (classifier != null)
            {
                if (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_SECURITYSCHEME))
                    return true;
                else
                {
                    // If the classifier is inherited from the Security Scheme in the meta model then return true
                    EA.Collection baseClasses = classifier.BaseClasses;
                    foreach (EA.Element baseClass in baseClasses)
                    {
                        if (baseClass.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_SECURITYSCHEME))
                            return true;
                    }

                }

            }
            return false;
        }

        public static bool filterCommunity(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier == null)
            {
                //logger.log("Filtering for Community: Ignoring" + e.Name);
            }
            if (classifier != null)
            {
                if (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_COMMUNITY))
                    return true;
                else
                {
                    //logger.log("FFS" + classifier.Name + " " + RoundTripAddInClass.RoundTripAddInClassClass.METAMODEL_COMMUNITY);
                }

            }
            return false;
        }

        public static bool filterTrait(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier == null)
            {
                //logger.log("Filtering for Community: Ignoring" + e.Name);
            }
            if (classifier != null)
            {
                if (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_TRAIT))
                    return true;
                else
                {
                    //logger.log("FFS" + classifier.Name + " " + RoundTripAddInClass.RoundTripAddInClassClass.METAMODEL_COMMUNITY);
                }

            }
            return false;
        }

        public static bool filterRequestExample(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null)
            {
                if (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_PLACEHOLDER))
                {
                    if (con.SupplierEnd.Role == "Request")
                    {
                        return true;
                    }

                }
            }
            return false;
        }
        public static bool filterResponseExample(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null)
            {
                if (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_PLACEHOLDER))
                {
                    if (con.SupplierEnd.Role == "Response")
                        return true;
                }
            }
            return false;
        }


        public static bool filterReleasePipeline(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier == null)
            {
                //logger.log("Filtering for Community: Ignoring" + e.Name);
            }
            if (classifier != null)
            {
                if (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_RELEASEPIPELINE))
                    return true;
                else
                {
                    //logger.log("FFS" + classifier.Name + " " + RoundTripAddInClass.RoundTripAddInClassClass.METAMODEL_COMMUNITY);
                }

            }
            return false;
        }

        public static bool filterEnvironment(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier == null)
            {
                //logger.log("Filtering for Community: Ignoring" + e.Name);
            }
            if (classifier != null)
            {
                if (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_ENVIRONMENT))
                    return true;
                else
                {
                    //logger.log("FFS" + classifier.Name + " " + RoundTripAddInClass.RoundTripAddInClassClass.METAMODEL_COMMUNITY);
                }

            }
            return false;
        }


        public static bool filterSample(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (!filterObject(Repository, con, e, classifier))
                return false;
            if (classifier == null)
            {
                //logger.log("Filtering for Sample: Ignoring" + e.Name);
            }
            if (classifier != null)
            {
                if (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_SAMPLE))
                    return true;
                else
                {
                    //logger.log("FFS" + classifier.Name + " " + RoundTripAddInClass.RoundTripAddInClassClass.METAMODEL_SAMPLE);
                }

            }
            return false;
        }


        public static bool filterSchema(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (e.Type.Equals(RoundTripAddIn.RoundTripAddInClass.EA_TYPE_CLASS))
                return true;
            return false;
        }

        public static bool filterClass(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (e.Type.Equals(RoundTripAddIn.RoundTripAddInClass.EA_TYPE_CLASS))
                return true;
            return false;
        }

        public static bool filterObject(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
        {
            if (classifier != null && e.Type.Equals(RoundTripAddIn.RoundTripAddInClass.EA_TYPE_OBJECT))
                return true;
            return false;
        }

            public static bool filterObjectNotClassifiedAsMethod(EA.Connector con, EA.Element e, EA.Element classifier)
            {
                if (classifier != null)
                {
                    //logger.log("FilterObjectNotClassifiedAsMethod Classifier:" + classifier.Name);
                    if (e.Type.Equals(RoundTripAddIn.RoundTripAddInClass.EA_TYPE_OBJECT) && (classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_METHOD)))
                        return true;
                }
                return false;
            }

            public static bool filterContentType(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
            {
                if (classifier != null && classifier.Name.Equals(RoundTripAddIn.RoundTripAddInClass.METAMODEL_CONTENTTYPE))
                    return true;
                return false;
            }

            public static bool filterDataItem(EA.Repository Repository, EA.Connector con, EA.Element e, EA.Element classifier)
            {
                if (e != null)
                {
                    if (DiagramManager.isVisible(con))
                        if (e.Stereotype.Contains(RoundTripAddIn.RoundTripAddInClass.METAMODEL_DATAITEM))
                            return true;
                }
                return false;
            }


            static public Boolean isCDMPackage(EA.Repository Repository, EA.Package package)
            {
                //MessageBox.Show("Checking CDM Package:" + package.Name);
                if (package.Name == "CommonDataModel")
                {
                    //MessageBox.Show("Is CDM Pkg");
                    return true;
                }
                else if (package.ParentID == 0)
                {
                    //MessageBox.Show("Is not CDM Pkg");
                    return false;
                }
                else
                {
                    EA.Package parentPackage = Repository.GetPackageByID(package.ParentID);
                    return isCDMPackage(Repository, parentPackage);
                }
            }

            static public void extractDiagramMetaData(Hashtable result, EA.Element root)
            {
            int level = 1;
            String prefix = "";
            String filename = "";
            String project = "";
            String intertype = "n";


            String runState = root.RunState;

            logger.log("RunState is:" + runState);

            Dictionary<string, RunState> rs = ObjectManager.parseRunState(runState);
            if (rs.ContainsKey(RoundTripAddInClass.HIERARCHY_LEVEL))
            {
                level = Int32.Parse(rs[RoundTripAddInClass.HIERARCHY_LEVEL].value);
                result.Add(RoundTripAddInClass.HIERARCHY_LEVEL, level);
                logger.log("Level is:" + level);
            }


            if (rs.ContainsKey(RoundTripAddInClass.PREFIX))
            {
                prefix = rs[RoundTripAddInClass.PREFIX].value;
                result.Add(RoundTripAddInClass.PREFIX, prefix);
                logger.log("Prefix is:" + prefix);

            }

            if (rs.ContainsKey(RoundTripAddInClass.INCLUDE_INTERTYPE))
            {
                intertype = rs[RoundTripAddInClass.INCLUDE_INTERTYPE].value;                    
                logger.log("Include Intertype Links is:" + intertype);
            }
            result.Add(RoundTripAddInClass.INCLUDE_INTERTYPE, intertype);


            if (rs.ContainsKey(RoundTripAddInClass.FILENAME))
                {
                    filename = rs[RoundTripAddInClass.FILENAME].value;
                }
                else
                {
                    filename = root.Name;
                }
                result.Add(RoundTripAddInClass.FILENAME, filename);
                logger.log("FileName is:" + filename);

                if (rs.ContainsKey(RoundTripAddInClass.PROJECT))
                {
                    project = rs[RoundTripAddInClass.PROJECT].value;
                    result.Add(RoundTripAddInClass.PROJECT, project);
                }
                else
                {
                    result.Add(RoundTripAddInClass.PROJECT, RoundTripAddInClass.EXPORT_PACKAGE);
                }
                logger.log("Project is:" + project);
            }


        static public EA.Element extractSelection(DiagramCache diagramCache,EA.Element root)
        {
            //String[] result = new String[0];

            //EA.TaggedValue tv = root.TaggedValues.GetByName(RoundTripAddInClass.PROPERTY_SELECTOR);

            //if (tv != null)
            //{
            //    logger.log("Selector:" + tv.Value);
            //    result = tv.Value.Split(',');
            //}
            //return result;
            
            EA.Element result = diagramCache.elementIDHash[root.ClassifierID];
            logger.log("Selector:" + result.Name);
            return result;
        }

            static public EA.Element findContainer(EA.Repository Repository, EA.Diagram diagram, DiagramCache diagramCache, String stereotype)
            {
                logger.log("Finding container for diagram:" + diagram.Name);

                IList<EA.Element> samples = MetaDataManager.diagramSamples(Repository, diagramCache.elementsList);
                foreach (EA.Element sample in samples)
                {
                    if (sample.Stereotype != null && sample.Stereotype == stereotype)
                    {
                        logger.log("Constraint is identified by Constraint stereotype");

                        return sample;
                    }
                }
                throw new ModelValidationException("Unable to find Object stereotyped as " + stereotype + " on the diagram");
            }
    }
    }