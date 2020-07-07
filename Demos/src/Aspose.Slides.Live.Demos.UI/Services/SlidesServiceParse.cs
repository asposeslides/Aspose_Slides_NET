using Aspose.Slides;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Aspose.Slides.Live.Demos.UI.Services
{
	public partial class SlidesService
	{
		/// <summary>
		/// Parse documents into text and image files, saves resulted file to out folder.
		/// Returns parsed parts details.
		/// </summary>
		/// <param name="outFolder">Output folder file. If value is null files not saved.</param>
		/// <param name="sourceFiles">Source slides files to proceed.</param>
		/// <returns>Extracted data details.</returns>
		public List<(string file, string text, string[] media)> Parser(string outFolder, params string[] sourceFiles)
		{
			var result = new List<(string file, string text, string[] media)>();
			var files = new List<string>();
			foreach (var sourceFile in sourceFiles)
			{
				if (Directory.Exists(sourceFile))
					files.AddRange(Directory.EnumerateFiles(sourceFile));
				else
					files.Add(sourceFile);
			}

			foreach (var sourceFile in files)
			{
				using (var presentation = new Presentation(sourceFile))
				{
					var textBuilder = new StringBuilder();

					foreach (var slide in presentation.Slides)
					{
						foreach (var shp in slide.Shapes)
						{
							if (shp is AutoShape ashp)
								textBuilder.AppendLine(ashp.TextFrame.Text);
						}

						var notes = slide.NotesSlideManager.NotesSlide?.NotesTextFrame?.Text;

						if (!string.IsNullOrEmpty(notes))
							textBuilder.AppendLine(notes);
					}

					var text = textBuilder.ToString();
					string outFilePath = null;
					if (outFolder != null)
					{
						outFilePath = Path.Combine(outFolder, $"{Path.GetFileNameWithoutExtension(sourceFile)}.txt");
						File.WriteAllText(outFilePath, text.ToString());
					}

					var images = presentation.Images.Select((e, i) => (type: "Image", contentType: e.ContentType, bytes: e.BinaryData, index: i));
					var videos = presentation.Videos.Select((e, i) => (type: "Video", contentType: e.ContentType, bytes: e.BinaryData, index: i));
					var audios = presentation.Audios.Select((e, i) => (type: "Audio", contentType: e.ContentType, bytes: e.BinaryData, index: i));

					var mediaDatas = images.Union(videos).Union(audios);

					var medias = mediaDatas.Select(e => $"{e.type}: {e.contentType} ({e.bytes.Length})");

					if (outFolder != null)
					{
						foreach (var media in mediaDatas)
						{
							string extension;
							try
							{
								extension = MimeTypeMap.GetExtension(media.contentType);
							}
							catch (ArgumentException)
							{
								switch (media.type)
								{
									case "Image":
										extension = ".jpeg";
										break;
									case "Video":
										extension = ".avi";
										break;
									case "Audio":
										extension = ".wav";
										break;

									default:
										extension = ".unknown";
										break;
								}
							}

							var mediaFilePath = Path.Combine(outFolder, $"{media.type}_{media.index + 1:00}{extension}");
							File.WriteAllBytes(mediaFilePath, media.bytes);
						}
					}

					result.Add((outFilePath, text, medias.ToArray()));
				}
			}

			return result;
		}

		///// <summary>
		///// Asynchronously parse documents into text and image files, saves resulted file to out folder.
		///// Returns parsed parts details.
		///// </summary>
		///// <param name="outFolder">Output folder file. If value is null files not saved.</param>
		///// <param name="sourceFiles">Source slides files to proceed.</param>
		///// <returns>Extracted data details.</returns>
		//public async Task<List<(string file, string text, string[] images)>> ParserAsync(string outFolder, params string[] sourceFiles)
		//	=> await Task.Run(() => Parser(outFolder, sourceFiles));
	}
}
