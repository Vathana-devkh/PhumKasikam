using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using PhumKasikam.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhumKasikam.Pages.Admin.Blog
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public List<PhumKasikam.Models.Blog> BlogsList { get; set; } = new List<PhumKasikam.Models.Blog>();

        [BindProperty]
        public PhumKasikam.Models.Blog NewBlog { get; set; } = new PhumKasikam.Models.Blog();

        [BindProperty]
        public PhumKasikam.Models.Blog EditBlog { get; set; } = new PhumKasikam.Models.Blog();

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ================= 01. READ ACTION =================
        public async Task OnGetAsync()
        {
            BlogsList = await _context.Blogs.OrderByDescending(b => b.CreatedAt).ToListAsync();
        }

        // ================= 02. CREATE ACTION =================
        public async Task<IActionResult> OnPostCreateBlogAsync(IFormFile? UploadImage, string? ImageUrlLink)
        {
            ModelState.Clear();

            // 🟢 ឆែកលក្ខខណ្ឌ៖ បើមានការផាស Link រូបភាព
            if (!string.IsNullOrEmpty(ImageUrlLink))
            {
                NewBlog.ImageUrl = ImageUrlLink.Trim();
            }
            // បើគ្មាន Link តែមានការជ្រើសរើសហ្វាយ Upload ពីកុំព្យូទ័រ
            else if (UploadImage != null && UploadImage.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(UploadImage.FileName);
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "blogs");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadImage.CopyToAsync(fileStream);
                }
                NewBlog.ImageUrl = "/images/blogs/" + uniqueFileName;
            }

            NewBlog.CreatedAt = DateTime.Now;
            _context.Blogs.Add(NewBlog);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // ================= 03. EDIT/UPDATE ACTION =================
        public async Task<IActionResult> OnPostEditBlogAsync(IFormFile? UploadImage, string? ImageUrlLink)
        {
            ModelState.Clear();

            var blogInDb = await _context.Blogs.FindAsync(EditBlog.Id);
            if (blogInDb == null) return NotFound();

            blogInDb.Title = EditBlog.Title;
            blogInDb.Content = EditBlog.Content;

            // 🟢 ឆែកលក្ខខណ្ឌពេល Edit៖ បើមានការផាស Link ថ្មី
            if (!string.IsNullOrEmpty(ImageUrlLink))
            {
                // លុបរូបភាពចាស់ពី Server ចោល (បើកាលមុនគាត់ធ្លាប់ Upload ហ្វាយ)
                if (!string.IsNullOrEmpty(blogInDb.ImageUrl) && !blogInDb.ImageUrl.StartsWith("http"))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, blogInDb.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }
                blogInDb.ImageUrl = ImageUrlLink.Trim();
            }
            // បើគ្មាន Link តែមានការជ្រើសរើសហ្វាយ Upload ថ្មី
            else if (UploadImage != null && UploadImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(blogInDb.ImageUrl) && !blogInDb.ImageUrl.StartsWith("http"))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, blogInDb.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(UploadImage.FileName);
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "blogs");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadImage.CopyToAsync(fileStream);
                }
                blogInDb.ImageUrl = "/images/blogs/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // ================= 04. DELETE ACTION =================
        public async Task<IActionResult> OnPostDeleteBlogAsync(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound();

            // 🟢 កែសម្រួល៖ លុបហ្វាយរូបភាពចេញពី Folder លុះត្រាតែវាជារូបភាពដែល Upload ពីក្នុងម៉ាស៊ីន (មិនមែនជា http Link ខាងក្រៅ)
            if (!string.IsNullOrEmpty(blog.ImageUrl) && !blog.ImageUrl.StartsWith("http"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, blog.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}