using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace CBookReader
{
    abstract class Book
    {
        private int currentPage;

        public IBitmapSourceLoader BitmapSouceLoader { get; set; }
        public virtual List<BitmapSource> Pages { get; set; }
        public virtual List<BrightContrast> PagesBrightContrast { get; set; }
        public abstract List<string> AviableImageFormats { get; }
        public abstract List<string> AviableArchiveFormats { get; }
        public abstract List<string> AviableComicFormats { get; }

        public int CurrentPage
        {
            set
            {
                this.currentPage = value;
                CurrentPageChanged?.Invoke();
            }

            get => this.currentPage;
        }

        public event Action CurrentPageChanged;

        protected Book()
        {
            this.Pages = new List<BitmapSource>();
            this.PagesBrightContrast = new List<BrightContrast>();
            this.CurrentPage = -1;
        }

        protected Book(List<BitmapSource> pages) : this()
        {
            this.Pages = pages;
        }

        public void Load(List<string> pathes)
        {
            this.Pages.AddRange(
                this.BitmapSouceLoader.Load(
                    pathes, this.AviableImageFormats));
        }

        public bool FirstPage()
        {
            if (this.Pages.Count > 0)
            {
                this.CurrentPage = 0;
                return true;
            }

            return false;
        }

        public bool PreviousPage()
        {
            if (this.Pages.Count > 0 &&
                this.CurrentPage > 0)
            {
                this.CurrentPage--;
                return true;
            }

            return false;
        }

        public bool NextPage()
        {
            if (this.Pages.Count > 0 &&
                this.CurrentPage < this.Pages.Count - 1)
            {
                this.CurrentPage++;
                return true;
            }

            return false;
        }

        public bool LastPage()
        {
            if (this.Pages.Count > 0)
            {
                this.CurrentPage = this.Pages.Count - 1;
                return true;
            }

            return false;
        }

        public bool GoToPage(int number)
        {
            if (this.Pages.Count > 0 && number > 0 &&
                number <= this.Pages.Count)
            {
                this.CurrentPage = number - 1;
                return true;
            }

            return false;
        }
    }
}
