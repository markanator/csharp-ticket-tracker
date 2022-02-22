using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Extensions;
using TheBugTracker.Models;
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
        private readonly UserManager<BTUser> _userManagerService;
        private readonly ICompanyInfoService _companyInfoService;

        public ProjectsController(ApplicationDbContext context, IRoleService roles, ILookupService lookup, IFileService file, IProjectService project, UserManager<BTUser> userManager, ICompanyInfoService companyInfo)
        {
            _context = context;
            _rolesService = roles;
            _lookupService = lookup;
            _fileService = file;
            _projectService = project;
            _userManagerService = userManager;
            _companyInfoService = companyInfo;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = await _projectService.GetAllProjectsByCompany(User.Identity.GetCompanyId().Value);
            return View(projects);
        }

        // GET: MyProjects
        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManagerService.GetUserId(User);
            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        // GET: /Projects/AllProjects
        public async Task<IActionResult> AllProjects()
        {

            List<Project> projects = new();
            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.Admin)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(User.Identity.GetCompanyId().Value);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompany(User.Identity.GetCompanyId().Value);
            }

            return View(projects);
        }

        // GET: /Projects/ArchivedProjects
        public async Task<IActionResult> ArchivedProjects()
        {
            List<Project> projects = await _projectService.GetArchivedProjectsByCompany(User.Identity.GetCompanyId().Value);

            return View(projects);
        }

        // GET: /Projects/AssignMembers
        public async Task<IActionResult> AssignMembers(int id)
        {
            var vm = new ProjectMembersViewModel();
            vm.Project = await _projectService.GetProjectByIdAsync(id, User.Identity.GetCompanyId().Value);
            vm.Members = new SelectList(await _companyInfoService.GetAllMembersAsync(User.Identity.GetCompanyId().Value), "Id", "FullName");

            return View(vm);
        }

        // POST: /Projects/AssignMembers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel pVm)
        {
            try
            {
                if (pVm.SelectedMembers.Count > 0)
                {
                    foreach (var member in pVm.SelectedMembers)
                    {
                        await _projectService.AddUserToProjectAsync(member, pVm.Project.Id);
                    }

                    return RedirectToAction(nameof(Details), new { id = pVm.Project.Id });
                }
                else
                {
                    var vm = new ProjectMembersViewModel();
                    vm.Project = await _projectService.GetProjectByIdAsync(pVm.Project.Id, User.Identity.GetCompanyId().Value);
                    vm.Members = new SelectList(await _companyInfoService.GetAllMembersAsync(User.Identity.GetCompanyId().Value), "Id", "FullName");

                    return View(vm);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // POST: Projects/UnassignedProjects
        [HttpGet]
        public async Task<IActionResult> UnassignedProjects()
        {
            var projects = await _projectService.GetUnassignedProjectsAsync(User.Identity.GetCompanyId().Value);

            return View(projects);
        }

        // Get: Projects/AssignPM
        [HttpGet]
        public async Task<IActionResult> AssignPM(int id)
        {
            AssignPMViewModel model = new();
            model.Project = await _projectService.GetProjectByIdAsync(id, User.Identity.GetCompanyId().Value);
            model.ProjectManagerList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), User.Identity.GetCompanyId().Value), "Id", "FullName");

            return View(model);
        }

        // Get: Projects/AssignPM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMID))
            {
                await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);

                return RedirectToAction(nameof(Details), new { id = model.Project.Id });
            }

            return RedirectToAction(nameof(AssignPM), new { id = model.Project.Id });
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
