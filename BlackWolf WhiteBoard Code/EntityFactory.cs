using BlackWolf_WhiteBoard_Code.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackWolf_WhiteBoard_Code
{
    public static class EntityFactory
    {

        public static EntityBase GetEntity(string entityName)
        {
            if (entityName == null)
                entityName = string.Empty;
            switch (entityName.ToLower())
            {

                case "bookableresourcebooking":
                    return new BookableResourceBooking();
                //case "msdyn_workorder":
                //    return new Workorder();

                    //case "msdyn_project":
                    //    return new Project();

                    //case "msdyn_projecttask":
                    //    return new ProjectTask();

                    //case "msdyn_resourceassignment":
                    //    return new ResourceAssignment();

            }
            return null;
        }
    }

}
