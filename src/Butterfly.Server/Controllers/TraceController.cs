﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Butterfly.Server.ViewModels;
using Butterfly.Storage;
using Butterfly.Storage.Query;
using Microsoft.AspNetCore.Mvc;
using Butterfly.DataContract.Tracing;
using  System.Linq;
using Butterfly.Server.Common;

namespace Butterfly.Server.Controllers
{
    [Route("api/[controller]")]
    public class TraceController : Controller
    {
        private readonly ISpanQuery _spanQuery;
        private readonly IMapper _mapper;

        public TraceController(ISpanQuery spanQuery, IMapper mapper)
        {
            _spanQuery = spanQuery;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<TraceViewModel>> Get(
            [FromQuery] string service, [FromQuery] string tags, [FromQuery] int? limit,
            [FromQuery] DateTime? startTimestamp, [FromQuery] DateTime? finishTimestamp,
            [FromQuery] int? minDuration, [FromQuery] int? maxDuration)
        {
            var query = new TraceQuery
            {
                Tags = tags,
                ServiceName = service,
                StartTimestamp = startTimestamp?.ToUniversalTime(),
                FinishTimestamp = finishTimestamp?.ToUniversalTime(),
                MinDuration = minDuration,
                MaxDuration = maxDuration,
                Limit = limit.GetValueOrDefault(10)
            };

            var data = await _spanQuery.GetTraces(query);
            var traceViewModels = _mapper.Map<List<TraceViewModel>>(data);

            foreach (var trace in traceViewModels)
            {
                var item = data.FirstOrDefault(x => x.TraceId == trace.TraceId);
                trace.Services = GetTraceServices(item);
            }

            return traceViewModels;
        }

        private List<TraceService> GetTraceServices(Trace trace)
        {
            var traceServices= new List<TraceService>();
            foreach (var span in trace.Spans)
            {
                traceServices.Add(new TraceService(ServiceHelpers.GetService(span)));
            }

            return traceServices;
        }
    }
}