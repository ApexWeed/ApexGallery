using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gallery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Gallery.Controllers
{
    public class ApiController : Controller
    {
        private readonly ApiSettings _apiSettings;

        public ApiController(IOptions<ApiSettings> apiSettings)
        {
            _apiSettings = apiSettings.Value;
        }

        // GET: /API/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /API/Languages
        public JsonResult Languages()
        {
            return Json(new LanguageModel(_apiSettings.Languages));
        }

        /// <summary>
        /// Creates a gallery image from a path.
        /// </summary>
        /// <param name="file">Path to the image.</param>
        /// <returns>Gallery image.</returns>
        private GalleryImage CreateImageInfo(string file)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var relativePath = file.Substring(_apiSettings.BasePath.Length);
            var imagePath = Path.Combine(_apiSettings.BaseUrl, relativePath).Replace('\\', '/');
            // Make sure the extension is changed to jpg.
            var thumbPath = Path.Combine(_apiSettings.ThumbUrl, Path.ChangeExtension(relativePath, _apiSettings.ThumbExtension)).Replace('\\', '/');
            var animated = Path.GetExtension(file).Equals(".gif");

            return new GalleryImage(name, imagePath, thumbPath, animated);
        }

        /// <summary>
        /// Gets the images in the specified directory.
        /// </summary>
        /// <param name="path">Path to directory.</param>
        /// <param name="start">How many images to skip.</param>
        /// <param name="count">How many images to return.</param>
        /// <returns>Model representing a set of images.</returns>
        private ImagesModel GetImages(string path, int start, int count)
        {
            // Enumerate images.
            var images = new List<GalleryImage>();
            var loaded = 0;
            var toSkip = start;
            
            // Sort by modification date.
            foreach (var file in new DirectoryInfo(path).GetFiles().OrderBy(p => p.LastWriteTimeUtc))
            {
                // Have to make sure it's an image before skipping it on the counter because webms and shit.
                if (!_apiSettings.ImageFormats.Contains(file.Extension))
                    continue;

                // Have to skip start of the directory if the user is loading page 2 etc.
                if (toSkip > 0)
                {
                    toSkip--;
                    continue;
                }

                loaded++;
                if (loaded > count)
                    break;

                images.Add(CreateImageInfo(file.FullName));
            }

            return new ImagesModel(start, images);
        }

        /// <summary>
        /// Checks a given path, replacing delimiters and disallowing relative paths.
        /// </summary>
        /// <param name="path">The path to check, or null if invalid.</param>
        /// <param name="error">The error, or null if valid.</param>
        /// <returns>Full path.</returns>
        private string CheckPath(string path, out ErrorModel error)
        {
            string fullPath;

            if (string.IsNullOrEmpty(path))
            {
                fullPath = _apiSettings.BasePath;
            }
            else
            {
                // Input paths have / replaced with : to seperate them from slashes in the url.
                fullPath = Path.Combine(_apiSettings.BasePath, path.Replace(':', '/'));

                // Windows woo.
                fullPath = fullPath.Replace('\\', '/');

                // Not sure this matters.
                while (fullPath.Contains("//"))
                {
                    fullPath = fullPath.Replace("//", "/");
                }

                // Avoid leaking info about the filesystem through relative paths.
                if (path.Contains("../"))
                {
                    error = new ErrorModel(301, "Relative paths not allowed.");
                    return null;
                }
            }

            if (!System.IO.Directory.Exists(fullPath))
            {
                error = new ErrorModel(402, "Directory not found.");
                return null;
            }

            error = null;
            return fullPath;
        }

        // GET: /API/{lang}/Images/{path}/{start}/{count}
        public async Task<JsonResult> Images(string lang, string path, int start, int count)
        {
            if (start < 0 || count < 0)
                return Json(new ErrorModel(403, "Invalid parameter."));

            if (!_apiSettings.Folders.ContainsKey(lang))
                return Json(new ErrorModel(101, "Language not found"));

            var fullPath = CheckPath(path, out ErrorModel error);
            if (fullPath == null)
                return Json(error);

            var images = GetImages(fullPath, start, count);
            await Task.Run(() => GenerateThumbnails(images.Images));

            return Json(images);
        }

        // GET: /API/{lang}/Directory/{path}
        public JsonResult Directory(string lang, string path)
        {
            if (!_apiSettings.Folders.ContainsKey(lang))
                return Json(new ErrorModel(401, "Language not found"));

            var currentLanguage = _apiSettings.Folders[lang];

            var fullPath = CheckPath(path, out ErrorModel error);
            if (fullPath == null)
                return Json(error);

            // Enumerate directories.
            var directories = new List<GalleryDirectory>();
            foreach (var dir in System.IO.Directory.EnumerateDirectories(fullPath))
            {
                var dirName = Path.GetFileName(dir);
                // Translate directory name if it's known.
                if (currentLanguage.ContainsKey(dirName))
                {
                    dirName = currentLanguage[dirName];
                }

                var relativePath = dir.Substring(_apiSettings.BasePath.Length).Replace('/', ':').Replace('\\', ':');

                directories.Add(new GalleryDirectory(dirName, relativePath));
            }

            return Json(new DirectoryModel(directories));
        }

        // GET: /API/Count/{path}
        public JsonResult Count(string path)
        {
            var fullPath = CheckPath(path, out ErrorModel error);
            if (fullPath == null)
                return Json(error);

            var count = System.IO.Directory.EnumerateFiles(fullPath).Select(Path.GetExtension).Count(extension => _apiSettings.ImageFormats.Contains(extension));

            return Json(new CountModel(count));
        }

        private int GenerateThumbnails(IEnumerable<GalleryImage> images)
        {
            var generatedCount = 0;
            Parallel.ForEach(images, image =>
            {
                if (GenerateThumbnail(image))
                {
                    generatedCount++;
                }
            });

            return generatedCount;
        }

        private bool GenerateThumbnail(GalleryImage image)
        {
            return GenerateThumbnail(Path.Combine(_apiSettings.BasePath, image.Path.Substring(_apiSettings.BaseUrl.Length)), Path.Combine(_apiSettings.ThumbPath, image.Thumb.Substring(_apiSettings.ThumbUrl.Length)));
        }

        private bool GenerateThumbnail(string sourceFile, string thumbnailFile)
        {
            // Don't generate existing thumbnails or attempt to thumbnail missing images.
            if (System.IO.File.Exists(thumbnailFile) || !System.IO.File.Exists(sourceFile))
                return false;

            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(thumbnailFile));

            var psi = new ProcessStartInfo
            {
                FileName = _apiSettings.ThumbCommand,
                Arguments = string.Format(_apiSettings.ThumbArgs, sourceFile, thumbnailFile, _apiSettings.ThumbSize)
            };

            using (var proc = new Process
            {
                StartInfo = psi
            })
            {
                proc.Start();
                proc.WaitForExit();
            }

            return true;
        }

        // GET: /API/ThumbDirectory/{path}/{recurse}
        public async Task<JsonResult> ThumbDirectory(string path, bool recurse)
        {
            var fullPath = CheckPath(path, out ErrorModel error);
            if (fullPath == null)
                return Json(error);

            var count = 0;
            var generatedCount = 0;

            foreach (var file in System.IO.Directory.EnumerateFiles(fullPath, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                // Have to make sure it's an image before skipping it on the counter because webms and shit.
                var extension = Path.GetExtension(file);
                if (!_apiSettings.ImageFormats.Contains(extension))
                {
                    continue;
                }

                count++;

                if (await Task.Run(() => GenerateThumbnail(CreateImageInfo(file))))
                {
                    generatedCount++;
                }
            }

            return Json(new ThumbnailModel(generatedCount, count));
        }

        // GET: /API/ThumbImages/{path}/{start}/{count}
        public async Task<JsonResult> ThumbImages(string path, int start, int count)
        {
            var fullPath = CheckPath(path, out ErrorModel error);
            if (fullPath == null)
                return Json(error);

            var images = await Task.Run(() => GetImages(fullPath, start, count).Images);
                
            return Json(new ThumbnailModel(GenerateThumbnails(images), images.Count));
        }
    }
}
