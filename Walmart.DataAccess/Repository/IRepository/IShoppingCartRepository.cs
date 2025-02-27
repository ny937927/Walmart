﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Walmart.DataAccess.Repository.IRepository;
using Walmart.Model.Models;

namespace Walmart.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart> 
    {
        void Update(ShoppingCart shoppingCart);

    }
}
