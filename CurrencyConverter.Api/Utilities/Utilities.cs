using CurrencyConverter.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyConverter.Api.Utilities
{
    public static class Utilities
    {

        public static PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>> Paginate(
           List<KeyValuePair<DateTime,ExchangeRateResponse>> ? allItems,
           HistoricalRatesRequest request)
        {
            var total = allItems.Count;
            var items = allItems
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            return new PagedResponse<KeyValuePair<DateTime, ExchangeRateResponse>>
            {
                Items = items,
                TotalItems = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

    }

}


