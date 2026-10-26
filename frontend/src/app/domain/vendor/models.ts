export interface VendorProfile {
  id: string;
  name: string;
  slug: string;
  status: string;
  description: string;
}

export interface VendorProduct {
  id: string;
  name: string;
  slug: string;
  status: string;
  variantCount: number;
  updatedAt: string;
  rejectionReason: string | null;
}

export interface CreateVariantInput {
  name: string;
  sku: string;
  priceKyat: number;
  weightGrams: number;
  stockOnHand: number;
}

export interface CreateProductDraft {
  categoryId: string;
  name: string;
  description: string;
  variants: CreateVariantInput[];
}

export interface CreatedProduct {
  id: string;
  slug: string;
}
