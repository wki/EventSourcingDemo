using System.Threading.Tasks;
using System.Web.Http;
using Designer.Domain;
using Wki.EventSourcing.Protocol.Statistics;

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

        [HttpGet, Route("person")]
        public Task<OfficeActorStatistics> PersonOfficeStatistics()
        {
            return designerService.GetPersonOfficeState();
        }
    
        [HttpGet, Route("hangtag")]
        public Task<OfficeActorStatistics> HangtagOfficeStatistics()
        {
            return designerService.GetHangtagOfficeState();
        }
    }
}
