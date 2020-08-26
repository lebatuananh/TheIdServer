﻿// Project: Aguafrommars/TheIdServer
// Copyright (c) 2020 @Olivier Lefebvre
using Aguacongas.IdentityServer.Store;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using entity = Aguacongas.IdentityServer.Store.Entity;

namespace Aguacongas.TheIdServer.BlazorApp.Components.ApiComponents
{
    public partial class ApiApiScope
    {
        private bool _isReadOnly;

        private readonly PageRequest _pageRequest = new PageRequest
        {
            Select = nameof(entity.ApiScope.Id),
            Take = 5
        };

        protected override bool IsReadOnly => _isReadOnly;

        protected override string PropertyName => "Id";

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            _isReadOnly = Entity.Id != null;
        }

        protected override async Task<IEnumerable<string>> GetFilteredValues(string term)
        {
            _pageRequest.Filter = $"contains({nameof(Entity.Id)},'{term}')";
            var response = await _store.GetAsync(_pageRequest)
                .ConfigureAwait(false);

            return response.Items.Select(c => c.Id);
        }

        protected override void SetValue(string inputValue)
        {
            Entity.ApiScopeId = inputValue;
        }
    }
}
