﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Articulate.Options;
using CookComputing.XmlRpc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;

namespace Articulate.Models
{
    public class PostModel : MasterModel
    {
        private PostAuthorModel _author;

        public PostModel(IPublishedContent content)
            : base(content)
        {
            PageTitle = Name + " - " + BlogTitle;
            PageDescription = Excerpt;
            PageTags = string.Join(",", Tags);
        }

        public IEnumerable<string> Tags
        {
            get
            {
                if (!UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
                {
                    var tags = this.GetPropertyValue<string>("tags");
                    return tags.IsNullOrWhiteSpace() ? Enumerable.Empty<string>() : tags.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    var tags = this.GetPropertyValue<IEnumerable<string>>("tags");
                    return tags ?? Enumerable.Empty<string>();
                }
            }
        }

        public IEnumerable<string> Categories
        {
            get
            {
                if (!UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
                {
                    var tags = this.GetPropertyValue<string>("categories");
                    return tags.IsNullOrWhiteSpace() ? Enumerable.Empty<string>() : tags.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    var tags = this.GetPropertyValue<IEnumerable<string>>("categories");
                    return tags ?? Enumerable.Empty<string>();
                }
            }
        }

        public bool EnableComments => Content.GetPropertyValue<bool>("enableComments", true);

        public PostAuthorModel Author
        {
            get
            {
                if (_author != null)
                {
                    return _author;
                }

                _author = new PostAuthorModel
                {
                    Name = Content.GetPropertyValue<string>("author", true)
                };

                //look up assocated author node if we can
                var authors = RootBlogNode?.Children(content => content.DocumentTypeAlias.InvariantEquals("ArticulateAuthors")).FirstOrDefault();
                var authorNode = authors?.Children(content => content.Name.InvariantEquals(_author.Name)).FirstOrDefault();
                if (authorNode != null)
                {
                    _author.Bio = authorNode.GetPropertyValue<string>("authorBio");
                    _author.Url = authorNode.GetPropertyValue<string>("authorUrl");

                    var imageVal = authorNode.GetPropertyValue<string>("authorImage");
                    _author.Image = !imageVal.IsNullOrWhiteSpace()
                        ? authorNode.GetCropUrl(propertyAlias: "authorImage", imageCropMode: ImageCropMode.Max) 
                        : null;

                    _author.BlogUrl = authorNode.Url;
                }

                return _author;
            }
        }

        public string Excerpt => this.GetPropertyValue<string>("excerpt");

        public DateTime PublishedDate => Content.GetPropertyValue<DateTime>("publishedDate");

        /// <summary>
        /// Some blog post may have an associated image
        /// </summary>
        public string PostImageUrl => Content.GetPropertyValue<string>("postImage");

        /// <summary>
        /// Cropped version of the PostImageUrl
        /// </summary>
        public string CroppedPostImageUrl => !PostImageUrl.IsNullOrWhiteSpace() 
            ? this.GetCropUrl("postImage", "wide") 
            : null;

        /// <summary>
        /// Social Meta Description
        /// </summary>
        public string SocialMetaDescription => this.GetPropertyValue<string>("socialDescription");

        public IHtmlString Body
        {
            get
            {
                if (this.HasProperty("richText"))
                {
                    return this.GetPropertyValue<IHtmlString>("richText");                    
                }
                else
                {
                    var val = this.GetPropertyValue<string>("markdown");
                    var md = new MarkdownDeep.Markdown();
                    UmbracoConfig.For.ArticulateOptions().MarkdownDeepOptionsCallBack(md);
                    return new MvcHtmlString(md.Transform(val));                    
                }
                
            }
        }

        public string ExternalUrl => this.GetPropertyValue<string>("externalUrl");
    }

}
