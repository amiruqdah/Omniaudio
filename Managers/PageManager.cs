using System;
using System.Collections.Generic;
namespace Omniaudio.Managers
{
    public sealed class PageManager
    {
        
        private static readonly Lazy<PageManager> pageManager = 
            new Lazy<PageManager>(() => new PageManager());

        public static PageManager Instance { get { return pageManager.Value;} }

        private Stack<IPage> pages;
        
        private PageManager()
        {
            pages = new Stack<IPage>();
        }

        public void AddPage(IPage page)
        {
            page.Init();
            pages.Push(page);   
        }

        public void Pop()
        {
            if (!isEmpty())
            {
                pages.Pop().Cleanup();
            }
        }

        public void Draw()
        {
            if(!isEmpty())
            pages.Peek().Update();
        }

        public void ChangePage(IPage page)
        {
            if(pages.Count != 0)
            {
                page.Cleanup();
                pages.Pop();
            }

            pages.Push(page);
            pages.Peek().Init();
        }

        private bool isEmpty()
        {
            if (pages.Count < 1)
                return true;
            else
                return false;
        }

    }
}
