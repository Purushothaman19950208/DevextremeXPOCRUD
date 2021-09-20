using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.Extensions.Configuration;
using DevextremeXPOCRUD.Models.XPO.PracticeDB;

namespace DevextremeXPOCRUD.Controllers
{
    // If you need to use Data Annotation attributes, attach them to this view model instead of an XPO data model.
    public class ProductsViewModel {
        public int ID { get; set; }
        public string NAME { get; set; }
        public int? PRICE { get; set; }
        public int? CategoryId { get; set; }
    }

    [Route("api/[controller]/[action]")]
    public class ProductsController : Controller
    {
        private UnitOfWork _uow;

        public ProductsController(UnitOfWork unitOfWork) {
            // Make sure that the Startup.cs file contains the XPO Data Layer initialization code.
            // For additional information, refer to the following help topic: https://docs.devexpress.com/XPO/14810/design-time-features/data-model-wizard#4
            this._uow = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions) {
            var xpproducts = _uow.Query<XPPRODUCT>().Select(i => new ProductsViewModel {
                ID = i.ID,
                NAME = i.NAME,
                PRICE = i.PRICE,
                CategoryId = i.CategoryId
            });

            // If underlying data is a large SQL table, specify PrimaryKey and PaginateViaPrimaryKey.
            // This can make SQL execution plans more efficient.
            // For more detailed information, please refer to this discussion: https://github.com/DevExpress/DevExtreme.AspNet.Data/issues/336.
            // loadOptions.PrimaryKey = new[] { "ID" };
            // loadOptions.PaginateViaPrimaryKey = true;

            return Json(await DataSourceLoader.LoadAsync(xpproducts, loadOptions));
        }

        [HttpPost]
        public async Task<IActionResult> Post(string values) {
            var model = new XPPRODUCT(_uow);
            var viewModel = new ProductsViewModel();
            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);

            PopulateViewModel(viewModel, valuesDict);

            if(!TryValidateModel(viewModel))
                return BadRequest(GetFullErrorMessage(ModelState));

            CopyToModel(viewModel, model);

            await _uow.CommitChangesAsync();

            return Json(new { model.ID });
        }

        [HttpPut]
        public async Task<IActionResult> Put(int key, string values) {
            var model = _uow.GetObjectByKey<XPPRODUCT>(key, true);
            if(model == null)
                return StatusCode(409, "Object not found");

            var viewModel = new ProductsViewModel {
                ID = model.ID,
                NAME = model.NAME,
                PRICE = model.PRICE,
                CategoryId = model.CategoryId
            };

            var valuesDict = JsonConvert.DeserializeObject<IDictionary>(values);
            PopulateViewModel(viewModel, valuesDict);

            if(!TryValidateModel(viewModel))
                return BadRequest(GetFullErrorMessage(ModelState));

            CopyToModel(viewModel, model);

            await _uow.CommitChangesAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task Delete(int key) {
            var model = _uow.GetObjectByKey<XPPRODUCT>(key, true);

            _uow.Delete(model);
            await _uow.CommitChangesAsync();
        }


        const string ID = nameof(XPPRODUCT.ID);
        const string NAME = nameof(XPPRODUCT.NAME);
        const string PRICE = nameof(XPPRODUCT.PRICE);
        const string CATEGORY_ID = nameof(XPPRODUCT.CategoryId);

        private void PopulateViewModel(ProductsViewModel viewModel, IDictionary values) {
            if(values.Contains(ID)) {
                viewModel.ID = Convert.ToInt32(values[ID]);
            }
            if(values.Contains(NAME)) {
                viewModel.NAME = Convert.ToString(values[NAME]);
            }
            if(values.Contains(PRICE)) {
                viewModel.PRICE = values[PRICE] != null ? Convert.ToInt32(values[PRICE]) : (int?)null;
            }
            if(values.Contains(CATEGORY_ID)) {
                viewModel.CategoryId = values[CATEGORY_ID] != null ? Convert.ToInt32(values[CATEGORY_ID]) : (int?)null;
            }
        }

        private void CopyToModel(ProductsViewModel viewModel, XPPRODUCT model) {
            model.ID = viewModel.ID;
            model.NAME = viewModel.NAME;
            model.PRICE = viewModel.PRICE;
            model.CategoryId = viewModel.CategoryId;
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = new List<string>();

            foreach(var entry in modelState) {
                foreach(var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return String.Join(" ", messages);
        }
    }
}