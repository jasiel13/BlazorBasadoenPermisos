/*aqui creamos el metodo customconfirm para editar a nuestro gusto el sweetalert y poder pasar los parametros necesarios*/
function CustomConfirm(titulo, mensaje, tipo) {
    //retornamos una promesa
    return new Promise((resolve) => {
        Swal.fire({
            title: titulo,//le pasamos el titulo
            text: mensaje,//le pasamos el mensaje
            icon: tipo,//le pasamos el tipo questions,info,warning
            showCancelButton: true,
            confirmButtonColor: '#02889B',
            cancelButtonColor: '#dc3545',
            confirmButtonText: 'Confirmar'
        }).then((result) => {
            //resolve true es que la promesa retorno exitosamente el valor en este caso true
            if (result.value) {
                resolve(true);
            }
            else {
                resolve(false);
            }
        });
    });
}

/*funcion para mostrar y ocultar contraseña en login y registro*/
function MostrarPassword()
{
    var cambio = document.getElementById("txtPassword");

    if (cambio.type == "password")
    {
        cambio.type = "text";
        $('.iconover').removeClass('oi oi-lock-locked').addClass('oi oi-lock-unlocked');
    }
    else
    {
        cambio.type = "password";
        $('.iconover').removeClass('oi oi-lock-unlocked').addClass('oi oi-lock-locked');
    }
};

/*funcion para mostrar animacion de numeros en acendente*/
function PureCounter() {

    const scr1 = document.createElement("script")
    scr1.src = "lib/purecounter_vanilla.js"

    setTimeout(function () {
        document.getElementById("counter2").classList.add("purecounter")
        let head = document.querySelector("head")
        head.appendChild(scr1)
    }, 2000);
    
};

/*ocultar boton derecho para evitar descargar archivos*/
function BloquearBotonDerecho() {

    $(document).bind("contextmenu", function (e) {
        return false;
    });   
};

/*hacer funcionar el carusel*/
function StarCarousel() {

    const swiper = new Swiper('.swiper', {
        direction: 'horizontal',
        loop: true,
        autoplay: true,
        pagination: {
            el: '.swiper-pagination',
        },
    });
};

function StarCarousel2() {

    const swiper = new Swiper('.swiper', {
        // Optional parameters
        direction: 'horizontal',
        loop: true,
        autoplay: true,
        spaceBetween: 5,
        loopedSlides: 5,
        slideToClickedSlide: true,
        // Navigation pagination dots
        pagination: {
            el: '.swiper-pagination',
        },
    });
};

/*hacer funcionar el menu*/
function AnimacionNavBar() {

    // Sticky Navbar
    $(window).scroll(function () {
        if ($(this).scrollTop() > 45) {
            $('.navbar').addClass('sticky-top shadow-sm');
        } else {
            $('.navbar').removeClass('sticky-top shadow-sm');
        }
    });

    // Smooth scrolling on the navbar links
    $(".navbar-nav a").on('click', function (event) {
        if (this.hash !== "") {
            event.preventDefault();

            $('html, body').animate({
                scrollTop: $(this.hash).offset().top - 45
            }, 1500, 'easeInOutExpo');

            if ($(this).parents('.navbar-nav').length) {
                $('.navbar-nav .active').removeClass('active');
                $(this).closest('a').addClass('active');
            }
        }
    });
};

/*hacer funcionar el boton up*/
function BotonUP() {

    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 100) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 1500, 'easeInOutExpo');
        return false;
    });
    
};

/*hacer owl carrusel*/
//function OwlCarousel() {
//    $(".owl-carousel").owlCarousel({
//        loop: true,
//        items: 1,
//        nav: false,
//        autoplay: true,
//        dots: true,
//        autoHeight: true
//    });
//};



