using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheBugTracker.Extensions;

namespace TheBugTracker.Models
{
	public class TicketAttachment
	{
		public int Id { get; set; }

		[DisplayName("Ticket")]
		public int TicketId { get; set; }

		[DisplayName("Team Member")] // person making the change
		public string UserId { get; set; }

		[DisplayName("File Date")] // created
		public DateTimeOffset Created { get; set; }

		[DisplayName("File Description")] // description of file to upload
		public string Description { get; set; }

		[DisplayName("File Name")]
		public string FileName { get; set; }

		[DisplayName("File Extension")]
		public string FileContentType { get; set; }

		public byte[] FileData { get; set; }

		[NotMapped]
		[DisplayName("Select a file")]
		[DataType(DataType.Upload)]
		[MaxFileSize(1024 * 1024)]
		[AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
		public IFormFile FormFile { get; set; }


		// navigation properties
		public virtual Ticket Ticket { get; set; }
		public virtual BTUser User { get; set; }
	}
}
