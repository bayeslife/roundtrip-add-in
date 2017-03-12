using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EA;

namespace UnitTestProject1.EAFacade
{
    public class EADiagramLink : EA.DiagramLink
    {
        public int ConnectorID{get;set;}
        

        public int DiagramID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Geometry
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string GetLastError()
        {
            throw new NotImplementedException();
        }

        public int InstanceID
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsHidden { get; set;}
        

        public EA.ObjectType ObjectType
        {
            get { throw new NotImplementedException(); }
        }

        public string Path
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Style
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int LineColor
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int LineWidth
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public LinkLineStyle LineStyle
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string TargetInstanceUID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SourceInstanceUID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool HiddenLabels
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int SuppressSegment
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Update()
        {
            return true;            
        }
    }
}
