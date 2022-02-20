using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Extensions;
using TheBugTracker.Models.Enums;
using TheBugTracker.Services.Interfaces;
using TheBugTracker.ViewModels;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _rolesService;
        private readonly ILookupService _lookupService;
        private readonly IFileService _fileService;
        private readonly IProjectService _projectService;

        public ProjectsController(ApplicationDbContext context, IRoleService roles, ILookupService lookup, IFileService file, IProjectService project)
        {
            _context = context;
            _rolesService = roles;
            _lookupService = lookup;
            _fileService = file;
            _projectService = project;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = await _projectService.GetAllProjectsByCompany(User.Identity.GetCompanyId().Value);
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel vm = new();
            // load selectLists for PMs and PriorityLists
            vm.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            vm.PriorityList = new SelectList(await _lookupService.GetAllProjectPrioritiesAsync(), "Id", "Name");

            return View(vm);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                try
                {
                    if (model.Project?.ImageFormFile != null)
                    {
                        // add aditional information if available
                        model.Project.ImageFileData = await _fileService.ConverFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }
                    model.Project.CompanyId = companyId;
                    // save changes
                    await _projectService.AddNewProjectAsync(model.Project);

                    // add PM if no one was chosen
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                // TODO: redirect to all projects
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Create");
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            AddProjectWithPMViewModel vm = new();

            vm.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            // load selectLists for PMs and PriorityLists
            vm.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            vm.PriorityList = new SelectList(await _lookupService.GetAllProjectPrioritiesAsync(), "Id", "Name");

            return View(vm);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                try
                {
                    if (model.Project?.ImageFormFile != null)
                    {
                        // add aditional information if available
                        model.Project.ImageFileData = await _fileService.ConverFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }

                    // save changes
                    await _projectService.UpdateProjectAsync(model.Project);

                    // add PM if no one was chosen
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                // TODO: redirect to all projects
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Edit));
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            var companyId = User.Identity.GetCompanyId().Value;
            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.ArchiveProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity.GetCompanyId().Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id, User.Identity.GetCompanyId().Value);
            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
