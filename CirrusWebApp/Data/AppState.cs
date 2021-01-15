using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CirrusWebApp.Data
{
    public class AppState
    {
        public bool UploadComplete { get; set; }
        public event Action OnChange;

        public void CheckUpload(bool isComplete)
        {
            UploadComplete = isComplete;
            NotifyStateChange();
        }

        private void NotifyStateChange() => OnChange?.Invoke();
    }
}
