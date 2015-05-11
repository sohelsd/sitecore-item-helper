using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Sitecore.NinjaTools
{
    public static class ItemHelper
    {
        public static string ItemUrl(Item item)
        {
            if (item == null) return string.Empty;
            if (item.Visualization.Layout == null) return string.Empty;
            var options = new Sitecore.Links.UrlOptions {LanguageEmbedding = Sitecore.Links.LanguageEmbedding.Never, AddAspxExtension = false, UseDisplayName = true, Language = Sitecore.Context.Language};
            var url = Sitecore.Links.LinkManager.GetItemUrl(item, options);
            return url ?? string.Empty;
        }

        public static string ItemUrl(Item item, string language)
        {
            if (item == null) return string.Empty;
            if (item.Visualization.Layout == null) return string.Empty;
            Language scLanguage = Language.Parse(language);
            var langItem = Sitecore.Context.Database.GetItem(item.ID, scLanguage);
            if (langItem == null) return string.Empty;
            var options = new Sitecore.Links.UrlOptions { LanguageEmbedding = Sitecore.Links.LanguageEmbedding.Never, AddAspxExtension = false, UseDisplayName = true };
            var url = Sitecore.Links.LinkManager.GetItemUrl(langItem, options);
            return url ?? string.Empty;
        }
        public static string ItemUrl(string itemGuid)
        {
            return ItemUrl(GetItem(itemGuid));
        }

        public static string GetLinkFieldUrl(Item item, string field)
        {
            if (item != null && !string.IsNullOrEmpty(field))
            {
                var linkfield = ((LinkField)item.Fields[field]);
                if (linkfield != null && linkfield.IsInternal)
                {
                    var lf = ((LinkField)item.Fields[field]).TargetItem;
                    return lf == null ? string.Empty : ItemUrl(lf);
                }
                if (linkfield != null && linkfield.IsMediaLink)
                {
                    var lf = ((LinkField)item.Fields[field]).TargetItem;

                    return lf == null ? string.Empty : Sitecore.StringUtil.EnsurePrefix('/', Sitecore.Resources.Media.MediaManager.GetMediaUrl(lf));
                }
                return linkfield != null ? linkfield.Url : string.Empty;
            }
            return string.Empty;
        }

        public static LinkField GetLinkField(Item item, string field)
        {
            if (item != null && !string.IsNullOrEmpty(field))
            {
                var linkfield = ((LinkField)item.Fields[field]);
                return linkfield;
            }
            return null;
        }

        public static string GetLinkFieldUrl(LinkField field)
        {
            if (field != null && field.IsInternal)
            {
                var lf = field.TargetItem;
                return lf == null ? string.Empty : ItemUrl(lf);
            }
            if (field != null && field.IsMediaLink)
            {
                var lf = field.TargetItem;
                return lf == null ? string.Empty : Sitecore.StringUtil.EnsurePrefix('/', Sitecore.Resources.Media.MediaManager.GetMediaUrl(lf));
            }
            if (field !=null && !field.IsInternal && !field.IsMediaLink)
            {
                return field.Url;
            }
            return string.Empty;
        }

        public static Item GetItem(string itemGuid)
        {
            if (!string.IsNullOrEmpty(itemGuid)  && (Utilities.Validators.IsGuid(itemGuid) || itemGuid.Contains("/")))
                return Sitecore.Context.Database.GetItem(itemGuid);
            if (!string.IsNullOrEmpty(itemGuid))
            {
                var guid = Guid.Parse(itemGuid);
                var id = ID.Parse(guid);
                return Sitecore.Context.Database.GetItem(id);
            }
            return null;
        }

        public static Item GetItemFromPath(string path)
        {
            return Sitecore.Context.Database.GetItem(path, Sitecore.Context.Language);
        }

        public static Item[] GetItemsFromPath(string path)
        {
            return Sitecore.Context.Database.SelectItems(path);
        }

        public static List<Item> GetItemsByTemplate(Item rootItem, string[] templates)
        {
            var res = from Item item in rootItem.Axes.GetDescendants()
                   where templates.Contains(item.TemplateID.ToString())
                   select item;
            return res.ToList();
        }

        public static Item GetHomeItem()
        {
            return GetItem(Sitecore.Context.Site.StartPath);
        }

        public static string GetImageUrl(string field)
        {
            return GetImageUrl(Sitecore.Context.Item, field);
        }

        public static string GetImageUrl(string datasource, string field)
        {
            var item = Sitecore.Context.Database.GetItem(datasource);
            return GetImageUrl(item, field);
        }

        public static string GetImageUrl(Item item, string field)
        {
            var url = string.Empty;
            if (item != null)
            {
                ImageField imgField = item.Fields[field];
                if (imgField != null && imgField.MediaItem != null)
                {
                    url = Sitecore.StringUtil.EnsurePrefix('/', Sitecore.Resources.Media.MediaManager.GetMediaUrl(imgField.MediaItem));
                }    
            }
            
            return url;
        }

        public static string GetImageAltText(Item item, string field)
        {
            var altText = string.Empty;
            if (item != null)
            {
                ImageField imgField = item.Fields[field];
                if (imgField != null && imgField.MediaItem != null)
                {
                    altText = imgField.Alt;
                }
            }
            return altText;
        }

        public static string GetFileUrl(Item item, string field)
        {
            var videofield = (FileField)item.Fields[field];
            if (string.IsNullOrEmpty(videofield.Src) || videofield.MediaItem == null) return string.Empty;
            return Sitecore.Resources.Media.MediaManager.GetMediaUrl(videofield.MediaItem).Replace(".ashx", ".swf");
        }

        public static string GetFileDownloadUrl(Item item, string field)
        {
            var fileField = (FileField) item.Fields[field];
            return fileField != null ? Sitecore.Resources.Media.MediaManager.GetMediaUrl(fileField.MediaItem) : string.Empty;
        }

        public static Item GetDropTreeItem(string field, Item item)
        {
            if (item != null && !string.IsNullOrEmpty(field))
            {
                if (item.Fields[field] != null)
                {
                    var i = ((Field)item.Fields[field]).Value;
                    return GetItem(i);   
                }

            }
            return null;
        }

        public static List<Item> GetTreeListItems(string field, Item item)
        {
            var  ids = ID.ParseArray(item[field], false);
            return ids.Select(id => GetItem(id.ToString())).ToList();
        }

        public static void Clear(this MultilistField listField)
        {
            foreach (string itemID in listField.Items)
            {
                listField.Remove(itemID);
            }
        }

        public static MultilistField FetchMultiListItems(Item item, string field)
        {
            MultilistField items = null;
            if (item != null)
            {
                items = item.Fields[field];
            }
            return items;
        }

        public static string GetMultiListItemNames(Item item, string field)
        {
            var fieldValue = new StringBuilder();
            var mlFieldValues = FetchMultiListItems(item, field).GetItems();
            var delimiter = "";
            foreach (Item f in mlFieldValues)
            {
                fieldValue.Append(delimiter);
                fieldValue.Append(f.Name.Replace(" ",""));
                delimiter = ";";
            }
            return fieldValue.ToString();
        }

        public static bool FieldContainsAnyId(Field itemField, ID[] itemIds)
        {
            return itemIds.Any(id => FieldContainsId(itemField, id));
        }

        public static bool FieldContainsId(Field itemField, ID itemId)
        {
            if (itemField == null || String.IsNullOrWhiteSpace(itemField.Value))
            {
                return false;
            }

            return
                itemField.Value.Split(new[] { '|' }).Any(
                    id => id.Equals(itemId.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static DateTime TryGetDate(this Field field)
        {
            DateTime dateValue = DateTime.MinValue;

            if (field != null)
            {
                DateField dateField = field;
                dateValue = dateField.DateTime;
            }

            return dateValue;
        }

        public static bool Checked(Item item, string field)
        {
            CheckboxField cb = item.Fields[field];
            if (cb != null)
            {
                return cb.Checked;
            }
            return false;
        }

        public static string TryGetValue(this Field field)
        {
            string value = String.Empty;

            if (field != null)
            {
                value = field.Value;
            }

            return value;
        }

        public static string FieldValue(string datasource, string field)
        {
            return GetItem(datasource)[field];
        }
    }
} 