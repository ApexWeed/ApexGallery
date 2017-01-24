using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Gallery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Gallery.Controllers
{
    public class APIController : Controller
    {
        private readonly APISettings _APISettings;

        public APIController(IOptions<APISettings> APISettings)
        {
            _APISettings = APISettings.Value;
        }

        // GET: /API/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /API/Languages
        public JsonResult Languages()
        {
            return Json(new LanguageModel(_APISettings.Languages));
        }

        private GalleryImage CreateImageInfo(string File)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(File);
            var relativePath = File.Substring(_APISettings.BasePath.Length);
            var imagePath = System.IO.Path.Combine(_APISettings.BaseURL, relativePath).Replace('\\', '/');
            // Make sure the extension is changed to jpg.
            var thumbPath = System.IO.Path.Combine(_APISettings.ThumbURL, System.IO.Path.ChangeExtension(relativePath, _APISettings.ThumbExtension)).Replace('\\', '/');
            var animated = System.IO.Path.GetExtension(File).Equals(".gif");

            return new GalleryImage(name, imagePath, thumbPath, animated);
        }

        private ImagesModel GetImages(string Path, int Start, int Count)
        {
            // Enumerate images.
            var images = new List<GalleryImage>();
            var loaded = 0;
            var toSkip = Start;
            foreach (var file in System.IO.Directory.EnumerateFiles(Path))
            {
                // Have to make sure it's an image before skipping it on the counter because webms and shit.
                var extension = System.IO.Path.GetExtension(file);
                if (!_APISettings.ImageFormats.Contains(extension))
                {
                    continue;
                }

                // Have to skip start of the directory if the user is loading page 2 etc.
                if (toSkip > 0)
                {
                    toSkip--;
                    continue;
                }

                loaded++;
                if (loaded > Count)
                {
                    break;
                }

                images.Add(CreateImageInfo(file));
            }

            return new ImagesModel(Start, images);
        }

        // GET: /API/{lang}/Images/{path}/{start}/{count}
        public async Task<JsonResult> Images(string Lang, string Path, int Start, int Count)
        {
            if (Start < 0 || Count < 0)
            {
                return Json(new ErrorModel(403, "Invalid parameter."));
            }

            if (_APISettings.Folders.ContainsKey(Lang))
            {
                var currentLanguage = _APISettings.Folders[Lang];
                var fullPath = string.Empty;

                if (Path == null || Path.Length == 0)
                {
                    fullPath = _APISettings.BasePath;
                    Path = string.Empty;
                }
                else
                {
                    // Input paths have / replaced with : to seperate them from slashes in the url.
                    fullPath = System.IO.Path.Combine(_APISettings.BasePath, Path.Replace(':', '/'));

                    // Windows woo.
                    fullPath.Replace('\\', '/');

                    // not sure this matters.
                    while (fullPath.Contains("//"))
                    {
                        fullPath.Replace("//", "/");
                    }

                    // Avoid leaking info about the filesystem through relative paths.
                    if (Path.Contains("../"))
                    {
                        return Json(new ErrorModel(301, "Relative paths not allowed."));
                    }
                }

                if (System.IO.Directory.Exists(fullPath))
                {
                    var images = GetImages(fullPath, Start, Count);
                    var generatedCount = await Task.Run(() => { return GenerateThumbnails(images.Images); });

                    return Json(images);
                }
                else
                {
                    return Json(new ErrorModel(402, "Directory not found."));
                }
            }
            else
            {
                return Json(new ErrorModel(101, "Language not found"));
            }
        }

        // GET: /API/{lang}/Directory/{path}
        public JsonResult Directory(string Lang, string Path)
        {
            if (_APISettings.Folders.ContainsKey(Lang))
            {
                var currentLanguage = _APISettings.Folders[Lang];
                var fullPath = string.Empty;

                if (Path == null || Path.Length == 0)
                {
                    fullPath = _APISettings.BasePath;
                    Path = string.Empty;
                }
                else
                {
                    // Avoid leaking info about the filesystem through relative paths.
                    if (Path.Contains(".."))
                    {
                        return Json(new ErrorModel(301, "Relative paths not allowed."));
                    }

                    // Input paths have / replaced with : to seperate them from slashes in the url.
                    fullPath = System.IO.Path.Combine(_APISettings.BasePath, Path.Replace(':', '/'));

                    // Windows woo.
                    fullPath.Replace('\\', '/');

                    // not sure this matters.
                    while (fullPath.Contains("//"))
                    {
                        fullPath.Replace("//", "/");
                    }
                }

                if (System.IO.Directory.Exists(fullPath))
                {
                    // Enumerate directories.
                    var directories = new List<GalleryDirectory>();
                    foreach (var path in System.IO.Directory.EnumerateDirectories(fullPath))
                    {
                        var dirName = System.IO.Path.GetFileName(path);
                        // Translate directory name if it's known.
                        if (currentLanguage.ContainsKey(dirName))
                        {
                            dirName = currentLanguage[dirName];
                        }

                        var relativePath = path.Substring(_APISettings.BasePath.Length).Replace('/', ':').Replace('\\', ':');

                        directories.Add(new GalleryDirectory(dirName, relativePath));
                    }

                    return Json(new DirectoryModel(directories));
                }
                else
                {
                    return Json(new ErrorModel(402, "Directory not found."));
                }
            }
            else
            {
                return Json(new ErrorModel(401, "Language not found"));
            }
        }

        // GET: /API/Count/{path}
        public JsonResult Count(string Path)
        {
            var fullPath = string.Empty;

            if (Path == null || Path.Length == 0)
            {
                fullPath = _APISettings.BasePath;
                Path = string.Empty;
            }
            else
            {
                // Avoid leaking info about the filesystem through relative paths.
                if (Path.Contains(".."))
                {
                    return Json(new ErrorModel(301, "Relative paths not allowed."));
                }

                // Input paths have / replaced with : to seperate them from slashes in the url.
                fullPath = System.IO.Path.Combine(_APISettings.BasePath, Path.Replace(':', '/'));

                // Windows woo.
                fullPath.Replace('\\', '/');

                // not sure this matters.
                while (fullPath.Contains("//"))
                {
                    fullPath.Replace("//", "/");
                }
            }

            if (System.IO.Directory.Exists(fullPath))
            {
                var count = 0;
                foreach (var file in System.IO.Directory.EnumerateFiles(fullPath))
                {
                    // Only count images.
                    var extension = System.IO.Path.GetExtension(file);
                    if (_APISettings.ImageFormats.Contains(extension))
                    {
                        count++;
                    }
                }

                return Json(new CountModel(count));
            }
            else
            {
                return Json(new ErrorModel(402, "Directory not found."));
            }
        }

        private int GenerateThumbnails(List<GalleryImage> Images)
        {
            var generatedCount = 0;
            Parallel.ForEach(Images, (image) =>
            {
                if (GenerateThumbnail(image))
                {
                    generatedCount++;
                }
            });

            return generatedCount;
        }

        private bool GenerateThumbnail(GalleryImage Image)
        {
            return GenerateThumbnail(System.IO.Path.Combine(_APISettings.BasePath, Image.Path.Substring(_APISettings.BaseURL.Length)), System.IO.Path.Combine(_APISettings.ThumbPath, Image.Thumb.Substring(_APISettings.ThumbURL.Length)));
        }

        private bool GenerateThumbnail(string SourceFile, string ThumbnailFile)
        {
            // Don't generate existing thumbnails or attempt to thumbnail missing images.
            if (System.IO.File.Exists(ThumbnailFile) || !System.IO.File.Exists(SourceFile))
            {
                return false;
            }

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ThumbnailFile));

            var psi = new ProcessStartInfo
            {
                FileName = _APISettings.ThumbCommand,
                Arguments = string.Format(_APISettings.ThumbArgs, SourceFile, ThumbnailFile, _APISettings.ThumbSize)
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
        public async Task<JsonResult> ThumbDirectory(string Path, bool Recurse)
        {
            var fullPath = string.Empty;

            if (Path == null || Path.Length == 0)
            {
                fullPath = _APISettings.BasePath;
                Path = string.Empty;
            }
            else
            {
                // Avoid leaking info about the filesystem through relative paths.
                if (Path.Contains(".."))
                {
                    return Json(new ErrorModel(301, "Relative paths not allowed."));
                }

                // Input paths have / replaced with : to seperate them from slashes in the url.
                fullPath = System.IO.Path.Combine(_APISettings.BasePath, Path.Replace(':', '/'));

                // Windows woo.
                fullPath.Replace('\\', '/');

                // not sure this matters.
                while (fullPath.Contains("//"))
                {
                    fullPath.Replace("//", "/");
                }
            }

            if (System.IO.Directory.Exists(fullPath))
            {
                var count = 0;
                var generatedCount = 0;

                foreach (var file in System.IO.Directory.EnumerateFiles(fullPath, "*", Recurse ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly))
                {
                    // Have to make sure it's an image before skipping it on the counter because webms and shit.
                    var extension = System.IO.Path.GetExtension(file);
                    if (!_APISettings.ImageFormats.Contains(extension))
                    {
                        continue;
                    }

                    count++;

                    if (await Task.Run(() => { return GenerateThumbnail(CreateImageInfo(file)); }))
                    {
                        generatedCount++;
                    }
                }

                return Json(new ThumbnailModel(generatedCount, count));
            }
            else
            {
                return Json(new ErrorModel(402, "Directory not found."));
            }
        }

        // GET: /API/ThumbImages/{path}/{start}/{count}
        public async Task<JsonResult> ThumbImages(string Path, int Start, int Count)
        {
            var fullPath = string.Empty;

            if (Path == null || Path.Length == 0)
            {
                fullPath = _APISettings.BasePath;
                Path = string.Empty;
            }
            else
            {
                // Input paths have / replaced with : to seperate them from slashes in the url.
                fullPath = System.IO.Path.Combine(_APISettings.BasePath, Path.Replace(':', '/'));

                // Windows woo.
                fullPath.Replace('\\', '/');

                // not sure this matters.
                while (fullPath.Contains("//"))
                {
                    fullPath.Replace("//", "/");
                }
                
                // Avoid leaking info about the filesystem through relative paths.
                if (Path.Contains("../"))
                {
                    return Json(new ErrorModel(301, "Relative paths not allowed."));
                }
            }

            if (System.IO.Directory.Exists(fullPath))
            {
                var images = await Task.Run(() => { return GetImages(fullPath, Start, Count).Images; });
                
                return Json(new ThumbnailModel(GenerateThumbnails(images), images.Count));
            }
            else
            {
                return Json(new ErrorModel(402, "Directory not found."));
            }
        }
    }
}
