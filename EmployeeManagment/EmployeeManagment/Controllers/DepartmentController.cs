using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment
{
    public class DepartmentController : Controller
    {
        public string List()
        {
            return "List action in department controller";
        }

        public string Details()
        {
            return "Details action in department controller";
        }   
    }
}
