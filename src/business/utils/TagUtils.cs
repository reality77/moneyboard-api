
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dal.Model;
using Microsoft.EntityFrameworkCore;

namespace business.extensions
{
    public static class TagExtensions
    {
        public static async Task<IEnumerable<Tag>> GetAllParentTagsAsync(this Tag tag, MoneyboardContext db)
        {
            Tag workingTag = tag;
            List<Tag> parentTags = new List<Tag>();

            db.Tags.Load();

            while(workingTag != null)
            {
                if(workingTag.ParentTagId != null)
                {
                    var parent = await db.Tags.SingleOrDefaultAsync(t => t.Id == workingTag.ParentTagId.Value);
                    parentTags.Add(parent);

                    workingTag = parent;
                }
                else
                    workingTag = null;
            }

            return parentTags;
        }

        public static async Task<IEnumerable<Tag>> GetAllSubTagsAsync(this Tag tag, MoneyboardContext db)
        {
            Tag workingTag = tag;
            List<Tag> subTags = new List<Tag>();

            var allTags = await db.Tags.Include(t => t.SubTags).ToListAsync();

            return await GetAllSubTagsInternalAsync(workingTag, db, allTags);
        }

        private static async Task<List<Tag>> GetAllSubTagsInternalAsync(Tag workingTag, MoneyboardContext db, IEnumerable<Tag> allTags)
        {
            var results = new List<Tag>();
            var subTags = allTags.SingleOrDefault(t => t.Id == workingTag.Id).SubTags.ToList();

            results.AddRange(subTags);

            foreach(var subTag in subTags)
                results.AddRange(await GetAllSubTagsInternalAsync(subTag, db, allTags));

            return results;
        }
    }
}
