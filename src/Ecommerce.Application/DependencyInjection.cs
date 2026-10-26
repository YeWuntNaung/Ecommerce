using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Catalog.Products.CreateDraft;
using Ecommerce.Application.Catalog.Products.Review;
using Ecommerce.Application.Catalog.Products.SubmitForReview;
using Ecommerce.Application.Catalog.Storefront;
using Ecommerce.Application.Common;
using Ecommerce.Application.Vendors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateProductDraftCommandValidator>();

        services.AddSingleton<IClock, SystemClock>();

        services.AddScoped<CreateProductDraftHandler>();
        services.AddScoped<SubmitProductForReviewHandler>();
        services.AddScoped<ApproveProductHandler>();
        services.AddScoped<RejectProductHandler>();

        services.AddScoped<GetStorefrontHandler>();
        services.AddScoped<ListStorefrontProductsHandler>();
        services.AddScoped<GetStorefrontProductHandler>();

        services.AddScoped<ListMyProductsHandler>();
        services.AddScoped<GetMyVendorHandler>();

        return services;
    }
}
