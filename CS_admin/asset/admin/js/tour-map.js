(function () {
  'use strict';

  var tours = Array.isArray(window.TOUR_MAP_DATA) ? window.TOUR_MAP_DATA : [];
  var config = window.TOUR_MAP_CONFIG || {};

  var map = null;
  var infoWindow = null;
  var tourLayers = {};       // idTour -> { polyline, markers: [], color }
  var allBounds = null;
  var selectedTourId = null;

  function escapeHtml(s) {
    return String(s == null ? '' : s)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
  }

  function buildMarkerLabel(order, color, isPaused) {
    // Paused stops: gray pin with "!" instead of number
    var pinFill = isPaused ? '#94a3b8' : color;
    var textColor = isPaused ? '#475569' : color;
    var label = isPaused ? '!' : String(order);
    var svg = '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="40" viewBox="0 0 32 40">' +
      '<path d="M16 0C7.16 0 0 7.16 0 16c0 11.5 16 24 16 24s16-12.5 16-24C32 7.16 24.84 0 16 0z" fill="' + pinFill + '"' + (isPaused ? ' opacity="0.75"' : '') + '/>' +
      '<circle cx="16" cy="16" r="10" fill="#fff"/>' +
      '<text x="16" y="20" text-anchor="middle" font-family="Inter, sans-serif" font-size="14" font-weight="700" fill="' + textColor + '">' + label + '</text>' +
      '</svg>';
    return {
      url: 'data:image/svg+xml;base64,' + btoa(svg),
      scaledSize: new google.maps.Size(32, 40),
      anchor: new google.maps.Point(16, 40),
    };
  }

  function buildInfoContent(tour, stop) {
    return '<div style="font-family: Inter, sans-serif; min-width: 180px;">' +
      '<div style="font-size: 11px; color: ' + tour.color + '; font-weight: 700; text-transform: uppercase;">' +
      escapeHtml(tour.ten) +
      '</div>' +
      '<div style="font-size: 14px; font-weight: 700; color: #0f172a; margin: 4px 0;">' +
      'Stop ' + stop.thuTu + ': ' + escapeHtml(stop.ten) +
      '</div>' +
      '<div style="font-size: 11px; color: #7f8ea3;">' +
      stop.lat.toFixed(5) + ', ' + stop.lng.toFixed(5) +
      '</div>' +
      '</div>';
  }

  function makePolyline(path, color, isFallback) {
    return new google.maps.Polyline({
      path: path,
      geodesic: false,
      strokeColor: color,
      strokeOpacity: isFallback ? 0.55 : 0.85,
      strokeWeight: isFallback ? 3 : 4,
      icons: isFallback ? [
        // Dashed for fallback (Directions API failed)
        { icon: { path: 'M 0,-1 0,1', strokeOpacity: 1, scale: 3 }, offset: '0', repeat: '12px' }
      ] : [
        { icon: { path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW, scale: 3, strokeColor: color }, offset: '50%', repeat: '120px' }
      ],
      map: map,
    });
  }

  // Goi Directions API de lay duong thuc te, fallback sang chim bay neu fail.
  function loadRoute(tour, layer, path, onDone) {
    if (path.length < 2) {
      if (onDone) onDone();
      return;
    }
    var ds = new google.maps.DirectionsService();
    var origin = path[0];
    var destination = path[path.length - 1];
    var waypoints = path.slice(1, -1).map(function (p) { return { location: p, stopover: true }; });

    // Directions API limit: 25 waypoints incl. origin/destination -> chia chunk neu can
    if (waypoints.length > 23) {
      console.warn('[tour-map] Tour ' + tour.idTour + ' co > 25 stop, fallback duong chim bay.');
      layer.polyline = makePolyline(path, tour.color, true);
      if (onDone) onDone();
      return;
    }

    ds.route({
      origin: origin,
      destination: destination,
      waypoints: waypoints,
      travelMode: google.maps.TravelMode.WALKING,
      optimizeWaypoints: false, // giu nguyen thu tu tour
    }, function (result, status) {
      if (status === 'OK' && result.routes && result.routes.length > 0) {
        var detailPath = [];
        result.routes[0].legs.forEach(function (leg) {
          leg.steps.forEach(function (step) {
            step.path.forEach(function (pt) { detailPath.push(pt); });
          });
        });
        layer.polyline = makePolyline(detailPath, tour.color, false);
      } else {
        console.warn('[tour-map] Directions ' + status + ' cho tour ' + tour.idTour + ', fallback duong chim bay.');
        layer.polyline = makePolyline(path, tour.color, true);
      }
      // Neu tour nay dang duoc select, ap emphasis ngay sau khi polyline ra
      if (selectedTourId === tour.idTour) {
        setLayerEmphasis(tour.idTour, true, false);
      } else if (selectedTourId !== null) {
        setLayerEmphasis(tour.idTour, false, true);
      }
      if (onDone) onDone();
    });
  }

  function renderTour(tour) {
    if (!tour.stops || tour.stops.length === 0) return;

    var layer = { color: tour.color, markers: [], polyline: null };
    var path = tour.stops.map(function (s) { return { lat: s.lat, lng: s.lng }; });

    // Markers (numbered) — render ngay, khong cho Directions
    tour.stops.forEach(function (stop) {
      var marker = new google.maps.Marker({
        position: { lat: stop.lat, lng: stop.lng },
        map: map,
        title: tour.ten + ' · Stop ' + stop.thuTu + ': ' + stop.ten,
        icon: buildMarkerLabel(stop.thuTu, tour.color),
        zIndex: 100,
      });
      marker.addListener('click', function () {
        if (!infoWindow) infoWindow = new google.maps.InfoWindow();
        infoWindow.setContent(buildInfoContent(tour, stop));
        infoWindow.open(map, marker);
        selectTour(tour.idTour);
      });
      layer.markers.push(marker);

      if (!allBounds) allBounds = new google.maps.LatLngBounds();
      allBounds.extend({ lat: stop.lat, lng: stop.lng });
    });

    tourLayers[tour.idTour] = layer;

    // Async load duong di theo road
    loadRoute(tour, layer, path);
  }

  function fitToBounds(bounds) {
    if (!bounds || bounds.isEmpty()) return;
    map.fitBounds(bounds, { top: 60, right: 60, bottom: 60, left: 60 });
  }

  function fitAll() {
    fitToBounds(allBounds);
  }

  function fitTour(idTour) {
    var layer = tourLayers[idTour];
    if (!layer) return;
    var b = new google.maps.LatLngBounds();
    layer.markers.forEach(function (m) { b.extend(m.getPosition()); });
    fitToBounds(b);
  }

  function setLayerEmphasis(idTour, isEmphasized, isFaded) {
    var layer = tourLayers[idTour];
    if (!layer) return;
    if (layer.polyline) {
      layer.polyline.setOptions({
        strokeOpacity: isFaded ? 0.18 : (isEmphasized ? 1 : 0.85),
        strokeWeight: isEmphasized ? 6 : 4,
        zIndex: isEmphasized ? 50 : 1,
      });
    }
    layer.markers.forEach(function (m) {
      m.setOpacity(isFaded ? 0.35 : 1);
      m.setZIndex(isEmphasized ? 200 : 100);
    });
  }

  function selectTour(idTour) {
    if (selectedTourId === idTour) {
      // Click again to deselect
      clearSelection();
      return;
    }
    selectedTourId = idTour;

    Object.keys(tourLayers).forEach(function (id) {
      var matching = parseInt(id, 10) === idTour;
      setLayerEmphasis(parseInt(id, 10), matching, !matching);
    });

    document.querySelectorAll('.tour-info-card').forEach(function (el) {
      var cardId = parseInt(el.dataset.idTour, 10);
      el.classList.toggle('is-active', cardId === idTour);
    });

    fitTour(idTour);

    // Scroll the active card into view
    var activeCard = document.querySelector('.tour-info-card[data-id-tour="' + idTour + '"]');
    if (activeCard) {
      activeCard.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
  }

  function clearSelection() {
    selectedTourId = null;
    Object.keys(tourLayers).forEach(function (id) {
      setLayerEmphasis(parseInt(id, 10), false, false);
    });
    document.querySelectorAll('.tour-info-card').forEach(function (el) {
      el.classList.remove('is-active');
    });
    if (infoWindow) infoWindow.close();
  }

  function wireSidebar() {
    document.querySelectorAll('.tour-info-card').forEach(function (card) {
      card.addEventListener('click', function (e) {
        // Skip if clicked on action buttons inside card
        if (e.target.closest('.tour-info-actions')) return;
        var id = parseInt(card.dataset.idTour, 10);
        if (id) selectTour(id);
      });
    });

    var btnFitAll = document.getElementById('tourFitAll');
    if (btnFitAll) btnFitAll.addEventListener('click', fitAll);

    var btnClear = document.getElementById('tourClearSelection');
    if (btnClear) btnClear.addEventListener('click', clearSelection);
  }

  window.initTourAdminMap = function () {
    var mapEl = document.getElementById('tourGoogleMap');
    if (!mapEl) return;

    map = new google.maps.Map(mapEl, {
      center: config.center || { lat: 10.762622, lng: 106.660172 },
      zoom: 15,
      mapId: config.mapId || 'DEMO_MAP_ID',
      mapTypeControl: false,
      streetViewControl: false,
      fullscreenControl: false,
      gestureHandling: 'greedy',
    });

    // Render all tours
    tours.forEach(renderTour);

    // Initial fit
    fitAll();

    wireSidebar();
  };
})();
