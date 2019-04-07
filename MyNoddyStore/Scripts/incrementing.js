$('.minus-btn').on('click', function (e) { /*todo remove this reference ??*/
    e.preventDefault();
    var $this = $(this);
    var $input = $this.closest('div').find('input');
    var value = parseInt($input.val());

    if (value > 1) {
        value = value - 1;
    } else {
        value = 0;
    }

    $input.val(value);

});

$('.plus-btn').on('click', function (e) {
    e.preventDefault();
    var $this = $(this);
    var $input = $this.closest('div').find('input');
    var value = parseInt($input.val());

    if (value < 100) {
        value = value + 1;
    } else {
        value = 100;
    }

    $input.val(value);
});
//$(function () {

//    $("form div").append('<div class="inc button">+</div><div class="dec button">-</div>');

//});


//$(function () {

//  $(".numbers-row").append('<div class="inc button">+</div><div class="dec button">-</div>');

//  $(".button").on("click", function() {

//    var $button = $(this);
//    var oldValue = $button.parent().find("input").val();

//    if ($button.text() == "+") {
//  	  var newVal = parseFloat(oldValue) + 1;
//  	} else {
//	   // Don't allow decrementing below zero
//      if (oldValue > 0) {
//        var newVal = parseFloat(oldValue) - 1;
//	    } else {
//        newVal = 0;
//      }
//	  }

//    $button.parent().find("input").val(newVal);

//  });

//});
