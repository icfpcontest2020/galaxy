using System;
using System.Threading;

using CosmicMachine;

using Microsoft.AspNetCore.Mvc;

using PlanetWars.Contracts.AlienContracts.Serialization;
using PlanetWars.Server.GalaxyPad;

namespace PlanetWars.Server.Controllers
{
    [Route("/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GalaxyPadController : Controller
    {
        private readonly GalaxyPadProcessService processService;
        private readonly GalaxyPadAlienServerClient client;

        public GalaxyPadController(GalaxyPadProcessService processService, GalaxyPadAlienServerClient client)
        {
            this.processService = processService;
            this.client = client;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View("GalaxyPad");
        }

        [HttpGet("/galaxy-pad/start/{progName}")]
        public ActionResult<GalaxyPadProcessApiModel> Get([FromRoute] string progName)
        {
            var apad = processService.TryStart(progName);
            if (apad == null)
                return NotFound(progName);
            while (apad.LastCommand == CommandType.SendRequest)
            {
                var response = client.Post(apad.Request!);
                apad = apad.ProcessResponse(response);
            }
            var nextPid = processService.Save(apad);
            return new GalaxyPadProcessApiModel(nextPid, apad.LastCommand, apad.Screens, apad.Memory.PrettyFormat(), (int)apad.TimeTaken.TotalMilliseconds);
        }

        [HttpGet("/galaxy-pad/start")]
        public ActionResult<string[]> GetStartPrograms()
        {
            return processService.GetStartPrograms();
        }

        [HttpPost("/galaxy-pad/process/{pid}/click")]
        public ActionResult<GalaxyPadProcessApiModel> Post([FromRoute] string pid, [FromBody] int[] xy)
        {
            var process = processService.FindProcess(pid);
            if (process == null)
                return NotFound(pid);
            var x = xy[0];
            var y = xy[1];
            APad apad = ProcessAPad(() => process.ProcessClick(x, y));
            var nextPid = processService.Save(apad!);
            return new GalaxyPadProcessApiModel(nextPid, apad.LastCommand, apad.Screens, apad.Memory.PrettyFormat(), (int)apad.TimeTaken.TotalMilliseconds);
        }

        [HttpPost("/galaxy-pad/process/{pid}/change")]
        public ActionResult<GalaxyPadProcessApiModel> Post([FromRoute] string pid, [FromBody] string memory)
        {
            var process = processService.FindProcess(pid);
            if (process == null)
                return NotFound(pid);
            Data? data = memory.ParseDataPrettyFormattedString();
            APad apad = ProcessAPad(() => process.ChangeMemory(data));
            var nextPid = processService.Save(apad);
            return new GalaxyPadProcessApiModel(nextPid, apad.LastCommand, apad.Screens, apad.Memory.PrettyFormat(), (int)apad.TimeTaken.TotalMilliseconds);
        }

        private APad ProcessAPad(Func<APad> getApad)
        {
            APad? apad = null;
            var thread = new Thread(() =>
                                    {
                                        try
                                        {
                                            apad = getApad();
                                            while (apad.LastCommand == CommandType.SendRequest)
                                            {
                                                var response = client.Post(apad.Request!);
                                                apad = apad.ProcessResponse(response);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex);
                                        }
                                    }, 512000000);
            thread.Start();
            thread.Join();
            return apad!;
        }
    }
}