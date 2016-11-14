using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Designer.Domain;
using Designer.Domain.PersonManagement.DTOs;

namespace Designer.Web
{
    [RoutePrefix("api/person")]
    public class PersonController : ApiController
    {
        public class RegisterInfo
        {
            public string Fullname { get; set; }
            public string Email { get; set; }

            public override string ToString()
            {
                return string.Format("[RegisterInfo: Fullname={0}, Email={1}]", Fullname, Email);
            }
        }

        private DesignerService designerService;

        public PersonController(DesignerService designerService)
        {
            this.designerService = designerService;
        }

        [HttpGet, Route("info")]
        public string Info() => "OK";

        [HttpPost, Route("register")]
        public void Register([FromBody]RegisterInfo registerInfo)
        {
            designerService.RegisterPerson(registerInfo.Fullname, registerInfo.Email);
        }

        [HttpGet, Route("list")]
        public Task<IEnumerable<PersonInfo>> List()
        {
            return designerService.ListPersons();
        }

        [HttpGet, Route("{id}")]
        public Task<PersonInfo> GetPersonState(int id)
        {
            return designerService.GetPersonState(id);
        }
    }
}
