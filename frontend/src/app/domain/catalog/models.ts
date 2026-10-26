export interface Storefront {
  id: string;
  name: string;
  slug: string;
  description: string;
}

export interface ProductSummary {
  id: string;
  name: string;
  slug: string;
  priceFromKyat: number;
  currency: string;
  vendorName: string;
  vendorSlug: string;
  categoryName: string;
}

export interface ProductVariant {
  id: string;
  name: string;
  sku: string;
  priceKyat: number;
  currency: string;
  weightGrams: number;
  stockOnHand: number;
}

export interface StorefrontProduct {
  id: string;
  vendorId: string;
  name: string;
  slug: string;
  description: string;
  categoryName: string;
  vendorName: string;
  vendorSlug: string;
  variants: ProductVariant[];
}

export interface Category {
  id: string;
  name: string;
  slug: string;
}
