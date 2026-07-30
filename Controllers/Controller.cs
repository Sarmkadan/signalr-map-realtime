using System;
using System.Threading;
using System.Threading.Tasks;

namespace Controllers
{
    public class Controller : Controller
    {
        // TODO: Add CancellationToken parameters to all async controller actions
        protected override async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            // TODO: Implement
        }
    }
}