using Microsoft.AspNetCore.Mvc;
using TECHNOBANKS.Data;
using TECHNOBANKS.Models;

namespace TECHNOBANKS.Controllers
{
    public class SolicitacoesController : Controller
    {
        private readonly AppDbContext _context;

        public SolicitacoesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var lista = _context.Solicitacoes.ToList();
            return View(lista);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Solicitacao solicitacao)
        {
            if (ModelState.IsValid)
            {
                _context.Solicitacoes.Add(solicitacao);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(solicitacao);
        }
    }
}