using System;
using System.Collections.Generic;
using System.Text;

namespace libicecast.Networking
{
    /// <summary>
    /// A representation of an IceCast stream
    /// </summary>
    public class IceCastStream
    {
        private readonly string contentType;
        private readonly bool isPublic;
        private readonly bool isPrivate;
        private readonly string name;
        private readonly string description;
        private readonly string genre;
        private readonly string url;
        public string ContentType { get => contentType; }
        public bool IsPublic { get => isPublic; }
        public bool IsPrivate { get => isPrivate; }
        public string Name { get => name; }
        public string Description { get => description; }
        public string Genre { get => genre; }
        public string URL { get => url; }

        /// <summary>
        /// A representation of an IceCast stream
        /// </summary>
        public IceCastStream(string contentType, bool isPublic, 
            bool isPrivate, string name, string description, 
            string genre, string url) 
        {
            this.contentType = contentType;
            this.isPublic = isPublic;
            this.isPrivate = isPrivate;
            this.name = name;
            this.description = description;
            this.genre = genre;
            this.url = url;
        }
    }
}