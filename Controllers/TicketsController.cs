using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Extensions;
using TheBugTracker.Models;
using TheBugTracker.Models.Enums;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
	public class TicketsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly ITicketService _ticketService;
		private readonly UserManager<BTUser> _userManagerService;
		private readonly IProjectService _projectService;
		private readonly ILookupService _lookupService;
		private readonly IFileService _fileService;

		public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager, IProjectService projectService, ILookupService lookup, ITicketService ticket, IFileService file)
		{
			_context = context;
			_userManagerService = userManager;
			_projectService = projectService;
			_lookupService = lookup;
			_ticketService = ticket;
			_fileService = file;
		}

		// GET: Tickets
		public async Task<IActionResult> Index()
		{
			var applicationDbContext = _context.Tickets
				.Include(t => t.DeveloperUser)
				.Include(t => t.OwnerUser)
				.Include(t => t.Project)
				.Include(t => t.TicketPriority)
				.Include(t => t.TicketStatus)
				.Include(t => t.TicketType)
				.OrderBy(t => t.Created);
			return View(await applicationDbContext.ToListAsync());
		}

		// GET: MyTickets
		public async Task<IActionResult> MyTickets()
		{
			var user = await _userManagerService.GetUserAsync(User);
			var tickets = await _ticketService.GetTicketsByUserIdAsync(user.Id, user.CompanyId);

			return View(tickets);
		}

		// GET: AllTickets
		public async Task<IActionResult> AllTickets()
		{
			var tickets = await _ticketService.GetAllTicketsByCompanyAsync(User.Identity.GetCompanyId().Value);

			if (User.IsInRole(nameof(Roles.Developer)) || User.IsInRole(nameof(Roles.Submitter)))
			{
				// filtered tickets view
				return View(tickets.Where(t => t.Archived == false));
			}

			return View(tickets);
		}

		// GET: ArchivedTickets
		public async Task<IActionResult> ArchivedTickets()
		{
			var tickets = await _ticketService.GetArchivedTicketsAsync(User.Identity.GetCompanyId().Value);

			return View(tickets);
		}

		// GET: UnassignedTickets
		[Authorize(Roles = "Admin,ProjectManager")]
		public async Task<IActionResult> UnassignedTickets()
		{
			try
			{
				var tickets = new List<Ticket>();

				if (User.IsInRole(nameof(Roles.Admin)))
				{
					tickets = await _ticketService.GetUnassignTicketsAsync(User.Identity.GetCompanyId().Value);
				}
				else
				{
					foreach (var ticket in tickets)
					{
						if (await _projectService.IsAssignedProjectManagerAsync(_userManagerService.GetUserId(User), ticket.Id))
						{
							tickets.Add(ticket);
						}
					}
				}
				return View(tickets);
			}
			catch (Exception)
			{

				throw;
			}
		}

		// GET: Tickets/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var ticket = await _ticketService.GetTicketByIdAsync(id.Value);

			if (ticket == null)
			{
				return NotFound();
			}

			return View(ticket);
		}

		// GET: Tickets/Create
		public async Task<IActionResult> Create()
		{
			var user = await _userManagerService.GetUserAsync(User);
			int companyId = User.Identity.GetCompanyId().Value;

			// projects have to be company/user+roles specific
			if (User.IsInRole(nameof(Roles.Admin)))
			{
				ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");
			}
			else
			{
				ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(user.Id), "Id", "Name");
			}

			ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
			ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetAllTicketTypesAsync(), "Id", "Name");

			return View();
		}

		// POST: Tickets/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId")] Ticket ticket)
		{
			BTUser user = await _userManagerService.GetUserAsync(User);
			if (ModelState.IsValid)
			{

				ticket.Created = DateTimeOffset.Now;
				ticket.OwnerUserId = user.Id; // this is how EFCore saves relationships
				ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(Models.Enums.TicketStatus.New))).Value;

				await _ticketService.AddNewTicketAsync(ticket);

				//TODO: TICKET HISTORY
				//TODO: NOTFICATIONs
				return RedirectToAction(nameof(Index));
			}

			if (User.IsInRole(nameof(Roles.Admin)))
			{
				ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(user.CompanyId), "Id", "Name");
			}
			else
			{
				ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(user.Id), "Id", "Name");
			}

			ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
			ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetAllTicketTypesAsync(), "Id", "Name");
			return View(ticket);
		}

		// GET: Tickets/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
			if (ticket == null)
			{
				return NotFound();
			}

			ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
			ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetAllTicketStatusAsync(), "Id", "Name", ticket.TicketStatusId);
			ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetAllTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
			return View(ticket);
		}

		// POST: Tickets/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
		{
			if (id != ticket.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				var user = await _userManagerService.GetUserAsync(User);
				try
				{
					ticket.Updated = DateTimeOffset.Now;
					await _ticketService.UpdateTicketAsync(ticket);

				}
				catch (DbUpdateConcurrencyException)
				{
					if (!(await TicketExists(ticket.Id)))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				// TODO: FIX VIEW
				// TODO: TICKET HISTORY
				return RedirectToAction(nameof(Index));
			}

			ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
			ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetAllTicketStatusAsync(), "Id", "Name", ticket.TicketStatusId);
			ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetAllTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

			return View(ticket);
		}

		// GET: Tickets/Delete/5
		public async Task<IActionResult> Archive(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
			if (ticket == null)
			{
				return NotFound();
			}

			return View(ticket);
		}

		// POST: Tickets/Delete/5
		[HttpPost, ActionName("Archive")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ArchiveConfirmed(int id)
		{
			var ticket = await _ticketService.GetTicketByIdAsync(id);
			await _ticketService.ArchiveTicketAsync(ticket);

			return RedirectToAction(nameof(Index));
		}

		// GET: Tickets/Restore/5
		public async Task<IActionResult> Restore(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
			if (ticket == null)
			{
				return NotFound();
			}

			return View(ticket);
		}

		// POST: Tickets/Restore/5
		[HttpPost, ActionName("Restore")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RestoreConfirmed(int id)
		{
			var ticket = await _ticketService.GetTicketByIdAsync(id);
			ticket.Archived = false;
			await _ticketService.UpdateTicketAsync(ticket);

			return RedirectToAction(nameof(Index));
		}

		// POST: Tickets/AddTicketComment
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketComment([Bind("Id, Comment,TicketId")] TicketComment ticketComment)
		{
			if (ModelState.IsValid)
			{
				try
				{
					ticketComment.UserId = _userManagerService.GetUserId(User);
					ticketComment.Created = DateTimeOffset.Now;

					await _ticketService.AddTicketCommentAsync(ticketComment);
				}
				catch (Exception)
				{

					throw;
				}
			}
			// redirect to correct details page
			return RedirectToAction(nameof(Details), new { id = ticketComment.TicketId });
		}

		// POST: Tickets/AddTicketAttachment
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
		{
			string statusMessage;

			if (ModelState.IsValid && ticketAttachment.FormFile != null)
			{
				ticketAttachment.FileData = await _fileService.ConverFileToByteArrayAsync(ticketAttachment.FormFile);
				ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
				ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

				ticketAttachment.Created = DateTimeOffset.Now;
				ticketAttachment.UserId = _userManagerService.GetUserId(User);

				await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
				statusMessage = "Success: New attachment added to Ticket.";
			}
			else
			{
				statusMessage = "Error: Invalid data.";

			}

			return RedirectToAction(nameof(Details), new { id = ticketAttachment.TicketId, message = statusMessage });
		}

		[HttpGet]
		public async Task<IActionResult> ShowFile(int id)
		{
			TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
			string fileName = ticketAttachment.FileName;
			byte[] fileData = ticketAttachment.FileData;
			string ext = Path.GetExtension(fileName).Replace(".", "");

			Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
			return File(fileData, $"application/{ext}");
		}

		private async Task<bool> TicketExists(int id)
		{
			return (await _ticketService.GetAllTicketsByCompanyAsync(User.Identity.GetCompanyId().Value)).Any(t => t.Id == id);
		}
	}
}
