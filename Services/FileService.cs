﻿using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
	public class FileService : IFileService
	{
		private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
		public string ConverByteArrayToFile(byte[] fileData, string extension)
		{
			try
			{
				string imageBase64Data = Convert.ToBase64String(fileData);
				return string.Format($"data:{extension};base64,{imageBase64Data}");
			}
			catch (System.Exception)
			{

				throw;
			}
		}

		public async Task<byte[]> ConverFileToByteArrayAsync(IFormFile file)
		{
			try
			{
				MemoryStream memoryStream = new MemoryStream();
				await file.CopyToAsync(memoryStream);
				byte[] byteFile = memoryStream.ToArray();

				memoryStream.Close();
				memoryStream.Dispose();

				return byteFile;

			}
			catch (Exception)
			{

				throw;
			}
		}

		public string FormatFileSize(long bytes)
		{
			int counter = 0;
			decimal fileSize = bytes;

			// KB is 1024B
			while (Math.Round(fileSize / 1024) >= 1)
			{
				fileSize /= bytes;
				counter++;
			}
			return String.Format("{0:n1}{1}", fileSize, suffixes[counter]);
		}

		public string GetFileIcon(string file)
		{
			string fileImage = "default";
			if (!string.IsNullOrWhiteSpace(file))
			{
				fileImage = Path.GetExtension(file).Replace(".", "");
				return $"/img/{fileImage}.png";
			}
			return fileImage;
		}
	}
}
