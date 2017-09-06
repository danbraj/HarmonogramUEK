using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UEK_Harmonogram
{
    class Zestaw
    {
        public List<Grupa> Groups { get; set; }
        public bool IsSeparatedGroups { get; set; }

        public Zestaw() { }

        public Zestaw(List<Grupa> groups)
        {
            IsSeparatedGroups = false;
            Groups = groups;
            for (int i = 0, j = groups.Count, c = 0; i < j; i++)
            {
                if (!groups[i].IsLanguageCourse)
                {
                    if (IsExistsMoreThanOnePrimaryGroup(ref c))
                    {
                        IsSeparatedGroups = true;
                        break;
                    }
                }
            }
        }

        private bool IsExistsMoreThanOnePrimaryGroup(ref int counter)
        {
            if (++counter > 1)
                return true;
            else
                return false;
        }
    }
}
