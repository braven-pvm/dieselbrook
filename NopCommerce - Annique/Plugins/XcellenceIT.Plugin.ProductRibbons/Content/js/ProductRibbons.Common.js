function ProductRibbonInit() {
    var productIds = [];

    // Add the product IDs for each product on the page.
    $(".product-item").each(function () {
        var productId = $(this).data("productid");
        if (productId) {
            productIds.push(productId);
        }
    });
    //if productsIds array not null then call ajax
    if (productIds.length > 0) {
        $.ajax({
            type: "POST",
            url: "/ProductRibbonsPublic/RetrieveProductsRibbons",
            data: { productIds: productIds },
            success: function (data) {
                if (!$.isEmptyObject(data)) {
                    $(function () {
                        var productselector = $('#product-ribbon-info').attr('data-productboxselector');
                        var productparent = $('#product-ribbon-info').attr('data-productpagepicturesparentcontainerselector');

                        if (productselector !== "") {
                            $(productselector).wrap('<div class="product-ribbon-wrapper"></div>');
                            $(productselector).each(function () {
                                var productId = undefined;
                                if ($(this).hasClass('item-box'))
                                    productId = $(this).find(".product-item").data("productid");
                                else
                                    productId = $(this).closest(".item-box").find(".product-item").data("productid");

                                if (productId !== undefined) {
                                    var productHtml = data[productId];
                                    if (productHtml) {
                                        $(this).after(productHtml);
                                    }
                                }

                            });
                        }
                        if (productparent !== "") {
                            $(productparent).wrap('<div class="product-ribbon-wrapper"></div>');
                            $(productparent).each(function () {
                                var productId = undefined;
                                if ($(this).hasClass(productparent))
                                    productId = $(this).closest(".product-essential").parent().data("productid");
                                else
                                    productId = $(this).closest(".product-essential").parent().data("productid");

                                if (productId !== undefined) {
                                    var productHtml = data[productId];
                                    if (productHtml) {
                                        $(this).after(productHtml);
                                    }
                                }
                            });
                        }
                    });
                }
            },
            error: function () {
                console.log("fail to load ribbon");
            },
            complete: function () {
            }
        });
    }
}

ProductRibbonInit();

$(CatalogProducts).on('loaded', function () {
    ProductRibbonInit();
});
