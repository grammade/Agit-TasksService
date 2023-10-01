using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TaskService.Entities;
using TaskService.Persistence;
using Microsoft.EntityFrameworkCore;
using TaskService.Dtos;
using AutoMapper.QueryableExtensions;
using PaginationHelper;
using TaskService.Services;
using Microsoft.AspNetCore.Authorization;

namespace TaskService.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly Context _context;
        private readonly IPageHelper _pageHelper;
        private readonly UserAuthorizationService _auth;
        protected string IP => (string)HttpContext.Connection.RemoteIpAddress.ToString() ?? "";

        public TasksController(
            Context context,
            IMapper mapper,
            IPageHelper pageHelper,
            UserAuthorizationService auth)
        {
            _context = context;
            _mapper = mapper;
            _pageHelper = pageHelper;
            _auth = auth;
        }

        [HttpGet]
        public async Task<ActionResult<Envelope<Entities.Task>>> GetTasks([FromQuery] PaginationDto paginationDto)
        {
            var userId = _auth.getAuthorizedUser().Id;

            var res = _pageHelper.GetPageAsync(
                _context.Tasks.Where(t => t.UserId == userId),
                paginationDto
            );
            return Ok(await res);
        }

        [HttpPost]
        public async Task<ActionResult<Entities.Task>> CreateTask([FromBody] CreateTaskRec model)
        {
            var userId = _auth.getAuthorizedUser().Id;
            var newTask = new Entities.Task
            {
                UserId = userId,
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate
            };
            var res = _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return Ok(newTask);
        }
    }
}
