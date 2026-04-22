(function () {
  var pois = Array.isArray(window.POI_ADMIN_MAP_DATA) ? window.POI_ADMIN_MAP_DATA : [];
  var config = window.POI_ADMIN_MAP_CONFIG || {};
  var map = null;
  var infoWindow = null;
  var activeStatus = 'all';
  var query = '';
  var selectedId = null;
  var markerEntries = [];
  var localEntries = [];
  var localState = null;
  var uiWired = false;
  var googleCallbackSeen = false;
  var googleAuthFailed = false;
  var visibleCache = null;
  var searchFrame = 0;
  var localCameraFrame = 0;
  var localCameraSyncPending = false;
  var googleCameraFrame = 0;
  var pendingGoogleCamera = null;

  function byId(id) {
    return document.getElementById(id);
  }

  function text(value) {
    return String(value == null ? '' : value);
  }

  function escapeHtml(value) {
    return text(value)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  function normalize(value) {
    return text(value)
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '');
  }

  function clamp(value, min, max) {
    var number = Number(value);
    if (!isFinite(number)) {
      number = min;
    }
    return Math.max(min, Math.min(max, number));
  }

  function normalizeHeading(value) {
    var heading = Number(value);
    if (!isFinite(heading)) {
      heading = 0;
    }
    return ((heading % 360) + 360) % 360;
  }

  function initials(name) {
    var parts = text(name).trim().split(/\s+/).filter(Boolean);
    if (!parts.length) {
      return 'GH';
    }

    return parts.slice(0, 2).map(function (part) {
      return part.charAt(0).toUpperCase();
    }).join('');
  }

  function detailUrl(poi) {
    return 'index1st.php?usecase=branchdetail2&idGianHang=' + encodeURIComponent(poi.id);
  }

  function searchableText(poi) {
    return normalize([
      poi.name,
      poi.address,
      poi.ownerName,
      poi.statusLabel,
      poi.id
    ].join(' '));
  }

  pois.forEach(function (poi) {
    poi._lat = Number(poi.lat);
    poi._lng = Number(poi.lng);
    poi._radius = Number(poi.radiusMeters || 10);
    poi._monthlyFee = Number(poi.monthlyFee || 0);
    poi._searchText = searchableText(poi);
  });

  function invalidateVisibleCache() {
    visibleCache = null;
  }

  function matchesFilter(poi) {
    var statusMatches = activeStatus === 'all' || poi.statusClass === activeStatus;
    var queryMatches = query === '' || (poi._searchText || searchableText(poi)).indexOf(query) !== -1;
    return statusMatches && queryMatches;
  }

  function visiblePois() {
    if (visibleCache && visibleCache.status === activeStatus && visibleCache.query === query) {
      return visibleCache.items;
    }

    visibleCache = {
      status: activeStatus,
      query: query,
      items: pois.filter(matchesFilter)
    };
    return visibleCache.items;
  }

  function markerClass(poi) {
    return 'poi-marker ' + escapeHtml(poi.statusClass || 'unknown') + (selectedId === poi.id ? ' selected' : '');
  }

  function poiPopupImage(poi, className) {
    if (!poi || !poi.imageUrl) {
      return '';
    }

    return '<div class="' + escapeHtml(className) + '">' +
      '<img src="' + escapeHtml(poi.imageUrl) + '" alt="" loading="lazy" />' +
    '</div>';
  }

  function buildMarkerContent(poi) {
    var wrap = document.createElement('div');
    wrap.className = markerClass(poi);
    wrap.innerHTML = '<div class="poi-marker-dot"><i class="fa-solid fa-store"></i></div>';
    return wrap;
  }

  function setMarkerMap(entry, targetMap) {
    if (!entry || !entry.marker) {
      return;
    }

    if ('map' in entry.marker) {
      entry.marker.map = targetMap;
    } else if (typeof entry.marker.setMap === 'function') {
      entry.marker.setMap(targetMap);
    }
  }

  function focusPoiCamera(poi) {
    if (!map || !poi) {
      return;
    }

    var center = { lat: poi._lat, lng: poi._lng };
    var currentZoom = typeof map.getZoom === 'function' ? Number(map.getZoom()) : 18;
    if (!isFinite(currentZoom)) {
      currentZoom = 18;
    }

    var camera = {
      center: center,
      zoom: Math.max(currentZoom, 18),
      tilt: currentTilt(),
      heading: currentHeading()
    };

    if (typeof map.moveCamera === 'function') {
      map.moveCamera(camera);
    } else {
      map.panTo(center);
      if (typeof map.setZoom === 'function' && camera.zoom !== currentZoom) {
        map.setZoom(camera.zoom);
      }
      if (typeof map.setTilt === 'function') {
        map.setTilt(camera.tilt);
      }
      if (typeof map.setHeading === 'function') {
        map.setHeading(camera.heading);
      }
    }

    syncCameraControls();
  }

  function refreshMarkerSelection() {
    markerEntries.forEach(function (entry) {
      if (entry.content) {
        entry.content.className = markerClass(entry.poi);
      }
    });
    refreshLocalSelection();
  }

  function openInfo(poi, marker) {
    if (localState) {
      openLocalInfo(poi);
      return;
    }

    if (!map || !infoWindow || !marker) {
      return;
    }

    selectedId = poi.id;
    refreshMarkerSelection();
    updateActiveListItem();

    infoWindow.setContent(
      '<div class="poi-info-window">' +
        poiPopupImage(poi, 'poi-info-media') +
        '<strong>' + escapeHtml(poi.name) + '</strong>' +
        '<span>' + escapeHtml(poi.address) + '</span>' +
        '<span>' + escapeHtml(poi.statusLabel) + ' - Radius ' + escapeHtml(poi.radiusMeters) + 'm - ' + escapeHtml(poi.monthlyFeeLabel) + '</span>' +
        '<a href="' + escapeHtml(detailUrl(poi)) + '">Mo chi tiet gian hang</a>' +
      '</div>'
    );

    if ('position' in marker && !marker.getPosition) {
      infoWindow.open({ map: map, anchor: marker });
    } else {
      infoWindow.open(map, marker);
    }

    focusPoiCamera(poi);
  }

  function createMarkers() {
    if (!map || !window.google) {
      return;
    }

    markerEntries.forEach(function (entry) {
      setMarkerMap(entry, null);
      if (entry.circle) {
        entry.circle.setMap(null);
      }
    });
    markerEntries = [];
    infoWindow = new google.maps.InfoWindow({ disableAutoPan: true });

    var canUseAdvanced = !!(config.mapId && config.mapId !== 'DEMO_MAP_ID' && google.maps.marker && google.maps.marker.AdvancedMarkerElement);

    pois.forEach(function (poi) {
      var position = { lat: poi._lat, lng: poi._lng };
      var content = canUseAdvanced ? buildMarkerContent(poi) : null;
      var marker = canUseAdvanced
        ? new google.maps.marker.AdvancedMarkerElement({
            map: map,
            position: position,
            title: poi.name,
            content: content
          })
        : new google.maps.Marker({
            map: map,
            position: position,
            title: poi.name
          });

      var circle = new google.maps.Circle({
        map: map,
        center: position,
        radius: poi._radius,
        strokeColor: poi.statusClass === 'active' ? '#16a34a' : (poi.statusClass === 'paused' ? '#f59e0b' : '#64748b'),
        strokeOpacity: 0.55,
        strokeWeight: 1,
        fillColor: poi.statusClass === 'active' ? '#16a34a' : (poi.statusClass === 'paused' ? '#f59e0b' : '#64748b'),
        fillOpacity: 0.08,
        clickable: false
      });

      marker.addListener('click', function () {
        openInfo(poi, marker);
      });

      markerEntries.push({
        poi: poi,
        marker: marker,
        circle: circle,
        content: content
      });
    });
  }

  function getLocalBounds() {
    var valid = pois.filter(function (poi) {
      return isFinite(poi._lat) && isFinite(poi._lng);
    });

    if (!valid.length) {
      return {
        minLat: 10.7622,
        maxLat: 10.7638,
        minLng: 106.6598,
        maxLng: 106.6613
      };
    }

    var minLat = Math.min.apply(null, valid.map(function (poi) { return poi._lat; }));
    var maxLat = Math.max.apply(null, valid.map(function (poi) { return poi._lat; }));
    var minLng = Math.min.apply(null, valid.map(function (poi) { return poi._lng; }));
    var maxLng = Math.max.apply(null, valid.map(function (poi) { return poi._lng; }));
    var latPad = Math.max((maxLat - minLat) * 0.32, 0.00055);
    var lngPad = Math.max((maxLng - minLng) * 0.32, 0.00055);

    return {
      minLat: minLat - latPad,
      maxLat: maxLat + latPad,
      minLng: minLng - lngPad,
      maxLng: maxLng + lngPad
    };
  }

  function localPoint(poi, bounds) {
    var lngSpan = Math.max(bounds.maxLng - bounds.minLng, 0.00001);
    var latSpan = Math.max(bounds.maxLat - bounds.minLat, 0.00001);
    var x = ((poi._lng - bounds.minLng) / lngSpan) * 100;
    var y = (1 - ((poi._lat - bounds.minLat) / latSpan)) * 100;

    return {
      x: Math.max(5, Math.min(95, x)),
      y: Math.max(7, Math.min(93, y))
    };
  }

  function statusColor(statusClass) {
    if (statusClass === 'active') {
      return '#16a34a';
    }
    if (statusClass === 'paused') {
      return '#d97706';
    }
    return '#64748b';
  }

  function localRadiusPixels(poi, bounds) {
    var centerLat = (bounds.minLat + bounds.maxLat) / 2;
    var metersWide = Math.max((bounds.maxLng - bounds.minLng) * 111320 * Math.cos(centerLat * Math.PI / 180), 1);
    var pxPerMeter = 980 / metersWide;
    return Math.max(34, Math.min(118, poi._radius * pxPerMeter * 2.2));
  }

  function localBuildingHeight(poi) {
    var fee = poi._monthlyFee;
    var radius = poi._radius;
    return Math.max(34, Math.min(118, 34 + (fee / 90000) + radius * 1.8));
  }

  function applyLocalCamera() {
    if (!localState || !localState.world) {
      return;
    }

    localState.heading = normalizeHeading(localState.heading);
    localState.tilt = clamp(localState.tilt, 0, 68);
    if (localCameraFrame) {
      localCameraSyncPending = true;
      return;
    }

    localCameraSyncPending = true;
    localCameraFrame = requestAnimationFrame(function () {
      localCameraFrame = 0;
      if (!localState || !localState.world || !localCameraSyncPending) {
        return;
      }
      localCameraSyncPending = false;
      localState.heading = normalizeHeading(localState.heading);
      localState.tilt = clamp(localState.tilt, 0, 68);
      applyLocalCameraNow();
    });
  }

  function applyLocalCameraNow() {
    if (!localState || !localState.world) {
      return;
    }

    localState.world.style.setProperty('--poi-local-heading', localState.heading + 'deg');
    localState.world.style.setProperty('--poi-local-tilt', localState.tilt + 'deg');
    localState.world.style.setProperty('--poi-local-heading-inverse', (-localState.heading) + 'deg');
    localState.world.style.setProperty('--poi-local-tilt-inverse', (-localState.tilt) + 'deg');
    localState.world.classList.toggle('flat', localState.tilt < 10);
    syncCameraControls();
  }

  function scheduleGoogleCamera(camera) {
    pendingGoogleCamera = camera;
    if (googleCameraFrame) {
      return;
    }

    googleCameraFrame = requestAnimationFrame(function () {
      googleCameraFrame = 0;
      if (!map || !pendingGoogleCamera) {
        pendingGoogleCamera = null;
        return;
      }

      var nextCamera = pendingGoogleCamera;
      pendingGoogleCamera = null;
      if (typeof map.moveCamera === 'function') {
        map.moveCamera(nextCamera);
      } else {
        if (typeof nextCamera.tilt !== 'undefined' && typeof map.setTilt === 'function') {
          map.setTilt(nextCamera.tilt);
        }
        if (typeof nextCamera.heading !== 'undefined' && typeof map.setHeading === 'function') {
          map.setHeading(nextCamera.heading);
        }
      }
      syncCameraControls();
    });
  }

  function currentTilt() {
    if (localState) {
      return clamp(localState.tilt, 0, 68);
    }
    if (map && typeof map.getTilt === 'function') {
      return clamp(map.getTilt() || 0, 0, 68);
    }
    return 62;
  }

  function currentHeading() {
    if (localState) {
      return normalizeHeading(localState.heading);
    }
    if (map && typeof map.getHeading === 'function') {
      return normalizeHeading(map.getHeading() || 0);
    }
    return 336;
  }

  function syncCameraControls() {
    var tiltSlider = byId('poiTiltSlider');
    var headingSlider = byId('poiHeadingSlider');
    var tiltValue = byId('poiTiltValue');
    var headingValue = byId('poiHeadingValue');
    var tilt = Math.round(currentTilt());
    var heading = Math.round(currentHeading());

    if (tiltSlider && document.activeElement !== tiltSlider) {
      tiltSlider.value = String(tilt);
    }
    if (headingSlider && document.activeElement !== headingSlider) {
      headingSlider.value = String(heading);
    }
    if (tiltValue) {
      tiltValue.textContent = String(tilt);
    }
    if (headingValue) {
      headingValue.textContent = String(heading);
    }
  }

  function setTiltValue(value) {
    var tilt = clamp(value, 0, 68);
    if (localState) {
      localState.tilt = tilt;
      applyLocalCamera();
      return;
    }
    if (map) {
      scheduleGoogleCamera({
        tilt: tilt,
        heading: currentHeading()
      });
    }
    syncCameraControls();
  }

  function setHeadingValue(value) {
    var heading = normalizeHeading(value);
    if (localState) {
      localState.heading = heading;
      applyLocalCamera();
      return;
    }
    if (map) {
      scheduleGoogleCamera({
        heading: heading,
        tilt: currentTilt()
      });
    }
    syncCameraControls();
  }

  function clearLocalPopup() {
    if (localState && localState.popup) {
      localState.popup.classList.remove('visible');
      localState.popup.innerHTML = '';
    }
  }

  function refreshLocalSelection() {
    localEntries.forEach(function (entry) {
      var selected = entry.poi.id === selectedId;
      if (entry.marker) {
        entry.marker.classList.toggle('selected', selected);
      }
      if (entry.building) {
        entry.building.classList.toggle('selected', selected);
      }
    });
  }

  function openLocalInfo(poi) {
    if (!localState || !localState.popup) {
      return;
    }

    selectedId = poi.id;
    refreshLocalSelection();
    updateActiveListItem();

    var entry = localEntries.find(function (item) {
      return item.poi.id === poi.id;
    });
    if (!entry) {
      return;
    }

    localState.popup.innerHTML =
      poiPopupImage(poi, 'poi-local-popup-media') +
      '<strong>' + escapeHtml(poi.name) + '</strong>' +
      '<span>' + escapeHtml(poi.address) + '</span>' +
      '<span>' + escapeHtml(poi.statusLabel) + ' - ' + escapeHtml(poi.radiusMeters) + 'm - ' + escapeHtml(poi.monthlyFeeLabel) + '</span>' +
      '<a href="' + escapeHtml(detailUrl(poi)) + '">Mo chi tiet</a>';

    localState.popup.style.left = Math.max(16, Math.min(78, entry.point.x)) + '%';
    localState.popup.style.top = Math.max(14, Math.min(74, entry.point.y)) + '%';
    localState.popup.classList.add('visible');
  }

  function renderLocalMarkers() {
    if (!localState || !localState.world) {
      return;
    }

    var world = localState.world;
    var bounds = getLocalBounds();
    localEntries = [];
    world.innerHTML =
      '<div class="poi-local-grid"></div>' +
      '<div class="poi-local-road road-main"></div>' +
      '<div class="poi-local-road road-cross"></div>' +
      '<div class="poi-local-road road-side-a"></div>' +
      '<div class="poi-local-road road-side-b"></div>';

    pois.forEach(function (poi, index) {
      var point = localPoint(poi, bounds);
      var color = statusColor(poi.statusClass);
      var radius = localRadiusPixels(poi, bounds);
      var height = localBuildingHeight(poi);
      var shift = (index % 2 === 0 ? -1 : 1) * (16 + (index % 3) * 8);

      var ring = document.createElement('div');
      ring.className = 'poi-local-ring ' + escapeHtml(poi.statusClass || 'unknown');
      ring.style.left = point.x + '%';
      ring.style.top = point.y + '%';
      ring.style.width = radius + 'px';
      ring.style.height = radius + 'px';
      ring.style.borderColor = color;
      world.appendChild(ring);

      var building = document.createElement('div');
      building.className = 'poi-local-building ' + escapeHtml(poi.statusClass || 'unknown');
      building.style.left = 'calc(' + point.x + '% + ' + shift + 'px)';
      building.style.top = 'calc(' + point.y + '% + 24px)';
      building.style.height = height + 'px';
      building.style.setProperty('--poi-building-color', color);
      world.appendChild(building);

      var marker = document.createElement('button');
      marker.type = 'button';
      marker.className = 'poi-local-marker ' + escapeHtml(poi.statusClass || 'unknown');
      marker.style.left = point.x + '%';
      marker.style.top = point.y + '%';
      marker.style.setProperty('--poi-marker-color', color);
      marker.setAttribute('aria-label', poi.name);
      marker.innerHTML =
        '<span class="poi-local-pin"><i class="fa-solid fa-store"></i></span>' +
        '<span class="poi-local-label">' + escapeHtml(poi.name) + '</span>';
      marker.addEventListener('click', function () {
        openLocalInfo(poi);
      });
      world.appendChild(marker);

      localEntries.push({
        poi: poi,
        marker: marker,
        ring: ring,
        building: building,
        point: point
      });
    });

    applyLocalMarkerVisibility();
    refreshLocalSelection();
  }

  function renderLocal3DMap() {
    var mapEl = byId('poiGoogleMap');
    if (!mapEl) {
      return;
    }

    map = null;
    markerEntries = [];
    infoWindow = null;
    mapEl.innerHTML =
      '<div class="poi-local-map">' +
        '<div class="poi-local-badge"><i class="fa-solid fa-cube"></i><span>3D</span></div>' +
        '<div class="poi-local-viewport">' +
          '<div class="poi-local-world"></div>' +
        '</div>' +
        '<div class="poi-local-popup" role="dialog" aria-live="polite"></div>' +
      '</div>';

    localState = {
      world: mapEl.querySelector('.poi-local-world'),
      popup: mapEl.querySelector('.poi-local-popup'),
      viewport: mapEl.querySelector('.poi-local-viewport'),
      heading: 336,
      tilt: 62
    };

    applyLocalCameraNow();
    wireLocalDrag(mapEl);
    renderLocalMarkers();
  }

  function wireLocalDrag(mapEl) {
    if (!mapEl || mapEl.getAttribute('data-local-drag-ready') === '1') {
      return;
    }
    mapEl.setAttribute('data-local-drag-ready', '1');

    var drag = null;

    mapEl.addEventListener('pointerdown', function (event) {
      if (!localState || event.button > 0) {
        return;
      }
      if (event.target.closest('.poi-local-marker, .poi-local-popup, .poi-map-controls, .poi-camera-panel')) {
        return;
      }

      drag = {
        x: event.clientX,
        y: event.clientY,
        heading: currentHeading(),
        tilt: currentTilt()
      };
      mapEl.classList.add('is-dragging');
      mapEl.setPointerCapture(event.pointerId);
      event.preventDefault();
    });

    mapEl.addEventListener('pointermove', function (event) {
      if (!drag || !localState) {
        return;
      }

      var dx = event.clientX - drag.x;
      var dy = event.clientY - drag.y;
      localState.heading = normalizeHeading(drag.heading + dx * 0.36);
      localState.tilt = clamp(drag.tilt - dy * 0.24, 0, 68);
      applyLocalCamera();
    });

    function endDrag(event) {
      if (!drag) {
        return;
      }
      drag = null;
      mapEl.classList.remove('is-dragging');
      if (event && typeof mapEl.releasePointerCapture === 'function') {
        try {
          mapEl.releasePointerCapture(event.pointerId);
        } catch (error) {
          // Pointer capture can already be released by the browser.
        }
      }
    }

    mapEl.addEventListener('pointerup', endDrag);
    mapEl.addEventListener('pointercancel', endDrag);
    mapEl.addEventListener('lostpointercapture', endDrag);
  }

  function fitVisiblePois() {
    if (localState) {
      localState.heading = 336;
      localState.tilt = 62;
      applyLocalCamera();
      clearLocalPopup();
      return;
    }

    if (!map || !window.google) {
      return;
    }

    var visible = visiblePois();
    if (!visible.length) {
      return;
    }

    var desiredTilt = Math.max(currentTilt(), 45);
    var desiredHeading = currentHeading();
    var bounds = new google.maps.LatLngBounds();
    visible.forEach(function (poi) {
      bounds.extend({ lat: poi._lat, lng: poi._lng });
    });

    map.fitBounds(bounds, 88);
    if (visible.length === 1) {
      map.setZoom(18);
    }

    setTimeout(function () {
      if (!map) {
        return;
      }
      if (typeof map.moveCamera === 'function') {
        map.moveCamera({
          tilt: desiredTilt,
          heading: desiredHeading
        });
      } else {
        if (typeof map.setTilt === 'function') {
          map.setTilt(desiredTilt);
        }
        if (typeof map.setHeading === 'function') {
          map.setHeading(desiredHeading);
        }
      }
      syncCameraControls();
    }, 250);
  }

  function applyLocalMarkerVisibility() {
    if (!localState) {
      return;
    }

    var visibleIds = {};
    visiblePois().forEach(function (poi) {
      visibleIds[poi.id] = true;
    });

    localEntries.forEach(function (entry) {
      var visible = !!visibleIds[entry.poi.id];
      entry.marker.classList.toggle('hidden', !visible);
      entry.ring.classList.toggle('hidden', !visible);
      entry.building.classList.toggle('hidden', !visible);
    });

    if (selectedId && !visibleIds[selectedId]) {
      clearLocalPopup();
    }
  }

  function applyMarkerVisibility() {
    if (localState) {
      applyLocalMarkerVisibility();
      return;
    }

    var visibleIds = {};
    visiblePois().forEach(function (poi) {
      visibleIds[poi.id] = true;
    });

    markerEntries.forEach(function (entry) {
      var visible = !!visibleIds[entry.poi.id];
      setMarkerMap(entry, visible ? map : null);
      if (entry.circle) {
        entry.circle.setMap(visible ? map : null);
      }
    });
  }

  function renderList() {
    var list = byId('poiList');
    var empty = byId('poiEmptyState');
    var counter = byId('poiVisibleCount');
    if (!list) {
      return;
    }

    var visible = visiblePois();
    var fragment = document.createDocumentFragment();
    list.innerHTML = '';

    visible.forEach(function (poi) {
      var button = document.createElement('button');
      button.type = 'button';
      button.className = 'poi-item' + (selectedId === poi.id ? ' active' : '');
      button.setAttribute('data-poi-id', String(poi.id));

      var thumb = poi.imageUrl
        ? '<img src="' + escapeHtml(poi.imageUrl) + '" alt="" />'
        : escapeHtml(initials(poi.name));

      button.innerHTML =
        '<div class="poi-thumb">' + thumb + '</div>' +
        '<div class="poi-body">' +
          '<div class="poi-title-row">' +
            '<strong>' + escapeHtml(poi.name) + '</strong>' +
            '<span class="poi-status ' + escapeHtml(poi.statusClass) + '">' + escapeHtml(poi.statusLabel) + '</span>' +
          '</div>' +
          '<div class="poi-address">' + escapeHtml(poi.address) + '</div>' +
          '<div class="poi-meta">' +
            '<span>#' + escapeHtml(poi.id) + '</span>' +
            '<span>' + escapeHtml(poi.ownerName) + '</span>' +
            '<span>' + escapeHtml(poi.radiusMeters) + 'm</span>' +
          '</div>' +
        '</div>';

      button.addEventListener('click', function () {
        var entry = markerEntries.find(function (item) {
          return item.poi.id === poi.id;
        });

        selectedId = poi.id;
        refreshMarkerSelection();
        updateActiveListItem();

        if (localState) {
          openLocalInfo(poi);
        } else if (entry) {
          setMarkerMap(entry, map);
          if (entry.circle) {
            entry.circle.setMap(map);
          }
          openInfo(poi, entry.marker);
        } else if (map) {
          focusPoiCamera(poi);
        }
      });

      fragment.appendChild(button);
    });

    list.appendChild(fragment);

    if (counter) {
      counter.textContent = String(visible.length);
    }

    if (empty) {
      empty.classList.toggle('visible', visible.length === 0);
    }
  }

  function updateActiveListItem() {
    var list = byId('poiList');
    if (!list) {
      return;
    }

    Array.prototype.forEach.call(list.querySelectorAll('.poi-item'), function (item) {
      item.classList.toggle('active', item.getAttribute('data-poi-id') === String(selectedId));
    });
  }

  function applyFilters(options) {
    renderList();
    applyMarkerVisibility();

    if (options && options.fit) {
      fitVisiblePois();
    }
  }

  function wireUi() {
    if (uiWired) {
      return;
    }
    uiWired = true;

    var searchInput = byId('poiMapSearch');
    if (searchInput) {
      searchInput.addEventListener('input', function () {
        if (searchFrame) {
          cancelAnimationFrame(searchFrame);
        }
        searchFrame = requestAnimationFrame(function () {
          searchFrame = 0;
          query = normalize(searchInput.value);
          invalidateVisibleCache();
          applyFilters({ fit: false });
        });
      });
    }

    Array.prototype.forEach.call(document.querySelectorAll('[data-poi-status]'), function (button) {
      button.addEventListener('click', function () {
        activeStatus = button.getAttribute('data-poi-status') || 'all';
        invalidateVisibleCache();
        Array.prototype.forEach.call(document.querySelectorAll('[data-poi-status]'), function (other) {
          other.classList.toggle('active', other === button);
        });
        applyFilters({ fit: true });
      });
    });

    var fitButton = byId('poiFitBounds');
    if (fitButton) {
      fitButton.addEventListener('click', fitVisiblePois);
    }

    var tiltSlider = byId('poiTiltSlider');
    if (tiltSlider) {
      tiltSlider.addEventListener('input', function () {
        setTiltValue(tiltSlider.value);
      });
    }

    var headingSlider = byId('poiHeadingSlider');
    if (headingSlider) {
      headingSlider.addEventListener('input', function () {
        setHeadingValue(headingSlider.value);
      });
    }

    var tiltButton = byId('poiToggleTilt');
    if (tiltButton) {
      tiltButton.addEventListener('click', function () {
        setTiltValue(currentTilt() > 10 ? 0 : 62);
      });
    }

    var rotateLeft = byId('poiRotateLeft');
    if (rotateLeft) {
      rotateLeft.addEventListener('click', function () {
        setHeadingValue(currentHeading() - 18);
      });
    }

    var rotateRight = byId('poiRotateRight');
    if (rotateRight) {
      rotateRight.addEventListener('click', function () {
        setHeadingValue(currentHeading() + 18);
      });
    }

    syncCameraControls();
  }

  function showMapMessage(message) {
    var mapEl = byId('poiGoogleMap');
    if (!mapEl) {
      return;
    }

    mapEl.innerHTML =
      '<div class="poi-map-loading">' +
        '<i class="fa-solid fa-map-location-dot"></i>' +
        '<span>' + escapeHtml(message) + '</span>' +
      '</div>';
  }

  function bootLocalMap() {
    wireUi();
    renderList();

    renderLocal3DMap();
  }

  window.renderPoiLocalMap = bootLocalMap;

  window.gm_authFailure = function () {
    googleAuthFailed = true;
    bootLocalMap();
  };

  window.initPoiAdminMap = function () {
    googleCallbackSeen = true;
    wireUi();
    renderList();

    var mapEl = byId('poiGoogleMap');
    if (!mapEl) {
      return;
    }

    if (googleAuthFailed || !config.hasApiKey || !window.google || !google.maps) {
      bootLocalMap();
      return;
    }

    localState = null;
    localEntries = [];

    var center = config.center || {};
    try {
      var fallbackPoi = pois.length ? pois[0] : {};
      var centerLat = Number(center.lat != null ? center.lat : fallbackPoi.lat);
      var centerLng = Number(center.lng != null ? center.lng : fallbackPoi.lng);
      if (!isFinite(centerLat)) {
        centerLat = 10.762622;
      }
      if (!isFinite(centerLng)) {
        centerLng = 106.660172;
      }

      var mapOptions = {
        center: {
          lat: centerLat,
          lng: centerLng
        },
        zoom: 18,
        tilt: 62,
        heading: 336,
        mapTypeId: 'roadmap',
        clickableIcons: false,
        gestureHandling: 'greedy',
        headingInteractionEnabled: true,
        tiltInteractionEnabled: true,
        cameraControl: true,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: true,
        rotateControl: true
      };

      if (google.maps.RenderingType && google.maps.RenderingType.VECTOR) {
        mapOptions.renderingType = google.maps.RenderingType.VECTOR;
      }

      if (config.mapId && config.mapId !== 'DEMO_MAP_ID') {
        mapOptions.mapId = config.mapId;
      }

      map = new google.maps.Map(mapEl, mapOptions);
      if (typeof map.setTiltInteractionEnabled === 'function') {
        map.setTiltInteractionEnabled(true);
      }
      if (typeof map.setHeadingInteractionEnabled === 'function') {
        map.setHeadingInteractionEnabled(true);
      }
      if (typeof map.setRenderingType === 'function' && google.maps.RenderingType && google.maps.RenderingType.VECTOR) {
        map.setRenderingType(google.maps.RenderingType.VECTOR);
      }
      if (typeof map.moveCamera === 'function') {
        map.moveCamera({
          center: mapOptions.center,
          zoom: mapOptions.zoom,
          tilt: mapOptions.tilt,
          heading: mapOptions.heading
        });
      }
      if (typeof map.addListener === 'function') {
        map.addListener('tilt_changed', syncCameraControls);
        map.addListener('heading_changed', syncCameraControls);
        map.addListener('renderingtype_changed', syncCameraControls);
      }

      createMarkers();
      applyFilters({ fit: true });
      syncCameraControls();
    } catch (error) {
      bootLocalMap();
    }
  };

  document.addEventListener('DOMContentLoaded', function () {
    wireUi();
    renderList();

    if (!config.hasApiKey) {
      bootLocalMap();
      return;
    }

    setTimeout(function () {
      if (!googleCallbackSeen && !map && !localState) {
        bootLocalMap();
      }
    }, 2400);
  });
})();
