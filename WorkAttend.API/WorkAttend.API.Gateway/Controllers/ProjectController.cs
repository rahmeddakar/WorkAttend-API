using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.CommonCode.Helpers;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectManager _projectManager;

        public ProjectController(IProjectManager projectManager)
        {
            _projectManager = projectManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<projectMod>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _projectManager.GetProjectsPageDataAsync(ctx);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("save-project")]
        public async Task<IActionResult> SaveProject([FromBody] projectClientList model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<projectClientList>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _projectManager.SaveProjectAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("{projectId:int}")]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = false
                });
            }

            var response = await _projectManager.DeleteProjectAsync(ctx, projectId);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}