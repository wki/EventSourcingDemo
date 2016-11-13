using System;
using System.Threading.Tasks;
using System.Web.Http;
using Designer.Domain;
using Wki.EventSourcing.Messages;

namespace Designer.Web
{
    [RoutePrefix("api/status")]
    public class StatusController : ApiController
    {
        private DesignerService designerService;

        public StatusController(DesignerService designerService)
        {
            this.designerService = designerService;
        }

        [HttpGet, Route("")]
        public Task<StatusReport> StatusReport()
        {
            return designerService.GetStatusReport();
        }
    }
}
