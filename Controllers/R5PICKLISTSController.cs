using Asp.Versioning;
using BE.Services;
using Microsoft.AspNetCore.Mvc;
using static BE.Services.R5PICKLISTSService;

namespace BE.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    public class R5PICKLISTSController: ControllerBase
    {

        [HttpGet("GetPartNotInPickListByWo")]
        public async Task<IActionResult> GetPartNotInPickListByWo(string wo)
        {

            var part = new PickListdb("Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            var result = await part.GetPartNotInPickListByWo(wo);

            return Ok(new { message = "OK", data = result });
        }

        [HttpPost("CreatePartInPickList")]
        public async Task<IActionResult> CreatePartInPickList(string code ,string wo)
        {
            var part = new PickListdb("Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            var result = await part.CreatePartInPickList(code, wo);

            return Ok(new { message = "OK", data = result });
        }

        [HttpGet("GetAllKy")]
        public async Task<IActionResult> GetKy(int currentpage, int pagesize, string? Search)
        {

            var part = new PickListdb("Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            var result = await part.GetKy(currentpage, pagesize, Search);

            return Ok(new { message = "OK", data = result });
        }

        [HttpGet("GetAllPerson")]
        public async Task<IActionResult> GetPhanLoai(int currentpage, int pagesize, string? Search, string org)
        {

            var part = new PickListdb("Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            var result = await part.GetPerson(currentpage, pagesize, Search, org);

            return Ok(new { message = "OK", data = result });
        }

        [HttpGet("GetAllPhanLoai")]
        public async Task<IActionResult> GetPhanLoai(int currentpage, int pagesize, string? Search)
        {

            var part = new PickListdb("Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            var result = await part.GetPhanLoai(currentpage, pagesize, Search);

            return Ok(new { message = "OK", data = result });
        }

        [HttpGet("GetAllStore")]
        public async Task<IActionResult> GetAllStore()
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new StoreApi(loginModel);
            var result = await part.GetAllStore();

            return Ok(new { message = "OK", data = result });
        }



        [HttpGet("GetAllPickList")]
        public async Task<IActionResult> GetAllPL(int currentpage, int pagesize, string? Search)
        {

            var part = new PickListdb("Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            var result = await part.GetAllPICKLIST(currentpage, pagesize, Search);

            return Ok(new { message = "OK", data = result });
        }


        [HttpGet("GetByWO")]
        public async Task<IActionResult> GetByWO(string pickticketnum)
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new PickListApi(loginModel);
            var result = await part.GetPICKLISTByWO(pickticketnum);

            return Ok(new { message = "OK", data = result });
        }


        [HttpDelete("DeletePickList")]
        public async Task<IActionResult> DeletePickList(string pickticketnum)
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new PickListApi(loginModel);
            var result = await part.DeletePICKLIST(pickticketnum);

            return Ok(new { message = "OK", data = result });
        }

        // part

        [HttpPost("Create")]
        public async Task<IActionResult> Create(R5PICKLISTSCreate dto)
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new PickListApi(loginModel);
            var result = await part.CreatePICKLIST(dto);

            return Ok(new { message = "OK", data = result });
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(R5PICKLISTSUpdate dto)
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new PickListApi(loginModel);
            var result = await part.UpdatePICKLIST(dto);

            return Ok(new { message = "OK", data = result });
        }

        [HttpGet("GetAllPart")]
        public async Task<IActionResult> GetAllPart(int currentpage, int pagesize, string? Search)
        {

            var part = new PickListdb("Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            var result = await part.GetPartList(currentpage, pagesize, Search);

            return Ok(new { message = "OK", data = result });
        }

        [HttpGet("GetAllPartPopUp")]
        public async Task<IActionResult> GetAllPartPopUp(int currentpage, int pagesize, string? Search)
        {

            var part = new PickListdb("Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            var result = await part.GetAllPartPopUp(currentpage, pagesize, Search);

            return Ok(new { message = "OK", data = result });
        }

        [HttpGet("GetPartByLine")]
        public async Task<IActionResult> GetPartByLine(string pickticketnum, string line)
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new PickListApi(loginModel);
            var result = await part.GetPartById(pickticketnum, line);

            return Ok(new { message = "OK", data = result });
        }

        [HttpPost("CreatePart")]
        public async Task<IActionResult> CreatePart(PartCreate dto)
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new PickListApi(loginModel);
            var result = await part.CreatePart(dto);

            return Ok(new { message = "OK", data = result });
        }

        [HttpPut("UpdatePart")]
        public async Task<IActionResult> UpdatePart(PartCreate dto)
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new PickListApi(loginModel);
            var result = await part.UpdatePart(dto);

            return Ok(new { message = "OK", data = result });
        }

        [HttpDelete("DeletePart")]
        public async Task<IActionResult> DeletePart(string pickticketnum, string line)
        {
            EAM_Helper eam = new EAM_Helper(
                "VIMICO-ADMIN",
                "HxGN@1234",
                "https://vimico.eam.vn/",
                "TRAIN",
                "5bc42f99-8c6e-4711-a2eb-fb467c3a8734",
                "Server=26.113.86.26,1433;Database=EAMDB_TRAIN;User Id=sa;Password=Ingr.123;MultipleActiveResultSets=True;TrustServerCertificate=True;"
            );

            var loginModel = await eam.Login();
            var part = new PickListApi(loginModel);
            var result = await part.DeletePart(pickticketnum, line);

            return Ok(new { message = "OK", data = result });
        }

    }
}
