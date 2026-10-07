#if DEBUG
using System;
using System.Web.Mvc;
using CodeScanner.Simulation;

namespace CodeScanner.Controllers
{
    /// <summary>
    /// Dedicated controller for monitoring, inspecting, and managing the local testing simulator.
    /// Completely separated from production controllers to ensure clean code structure.
    /// Excluded from Release / published builds via #if DEBUG.
    /// </summary>
    public class SimulatorController : Controller
    {
        // GET: /Simulator
        [HttpGet]
        public ActionResult Index()
        {
            return Content(
                "<html><head><title>Jig Simulator Monitor</title>" +
                "<style>body{font-family:Arial,sans-serif;margin:40px;background:#f8f9fa;color:#333;}" +
                ".card{background:#fff;padding:25px;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);max-width:800px;margin:auto;}" +
                "h2{color:#007bff;margin-top:0;}pre{background:#272822;color:#f8f8f2;padding:15px;border-radius:5px;overflow-x:auto;}" +
                ".badge{background:#28a745;color:#fff;padding:4px 8px;border-radius:4px;font-size:12px;}" +
                "button{background:#007bff;color:#fff;border:none;padding:8px 16px;border-radius:4px;cursor:pointer;}" +
                "</style></head><body>" +
                "<div class='card'>" +
                "<h2>Testing Jig Simulator Monitor <span class='badge'>Active (Debug Only)</span></h2>" +
                "<p>This controller manages the mock hardware jig. Production controllers (ComPortController) delegate to <code>JigSimulationEngine</code> without carrying any mock strings.</p>" +
                "<hr/>" +
                "<h4>Available Endpoints:</h4>" +
                "<ul>" +
                "<li><code>GET /Simulator/Status?barcode=VTPX15YJZ1K1A007315</code> - View current simulation step and last generated frame.</li>" +
                "<li><code>POST /Simulator/Reset</code> - Reset all simulation counters to Step 0.</li>" +
                "</ul>" +
                "<h4>Local Testing Instructions:</h4>" +
                "<p>On the <a href='/'>Index Page</a>, select Port: <b>SIMULATOR</b>, enter barcode: <b>VTPX15YJZ1K1A007315</b>, and click <b>Check</b>.</p>" +
                "</div></body></html>",
                "text/html"
            );
        }

        // GET: /Simulator/Status?barcode=...
        [HttpGet]
        public JsonResult Status(string barcode = null)
        {
            int currentStep = JigSimulationEngine.GetCurrentStep(barcode);
            string lastFrame = JigSimulationEngine.GetLastFrame(barcode);

            return Json(new
            {
                success = true,
                port = JigSimulationEngine.SimulatorPortName,
                barcode = barcode ?? "(all)",
                currentStep = currentStep,
                stepDelayMs = JigSimulationEngine.StepDelayMs,
                lastFrame = lastFrame,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: /Simulator/SetDelay?delayMs=1500
        [HttpPost]
        public JsonResult SetDelay(int delayMs)
        {
            if (delayMs < 0) delayMs = 0;
            JigSimulationEngine.StepDelayMs = delayMs;
            return Json(new
            {
                success = true,
                stepDelayMs = delayMs,
                message = $"Step delay updated to {delayMs} ms ({(delayMs / 1000.0):0.1} seconds)."
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: /Simulator/Reset
        [HttpPost]
        public JsonResult Reset(string barcode = null)
        {
            JigSimulationEngine.Reset(barcode);
            return Json(new
            {
                success = true,
                message = string.IsNullOrWhiteSpace(barcode)
                    ? "All simulation counters reset to Step 0."
                    : $"Simulation counter for barcode '{barcode}' reset to Step 0."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
#endif
