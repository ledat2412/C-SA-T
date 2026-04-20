(function () {
  var pois = Array.isArray(window.POI_ADMIN_MAP_DATA) ? window.POI_ADMIN_MAP_DATA : [];
  var config = window.POI_ADMIN_MAP_CONFIG || {};
  var map = null;
  var infoWindow = null;
  var activeStatus = 'all';
  var query = '';
  var selectedId = null;
  var markerEntries = [];

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

  function matchesFilter(poi) {
    var statusMatches = activeStatus === 'all' || poi.statusClass === activeStatus;
    var queryMatches = query === '' || searchableText(poi).indexOf(query) !== -1;
    return statusMatches && queryMatches;
  }

  function visiblePois() {
    return pois.filter(matchesFilter);
  }

  function markerClass(poi) {
    return 'poi-marker ' + escapeHtml(poi.statusClass || 'unknown') + (selectedId === poi.id ? ' selected' : '');
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

  function refreshMarkerSelection() {
    markerEntries.forEach(function (entry) {
      if (entry.content) {
        entry.content.className = markerClass(entry.poi);
      }
    });
  }

  function openInfo(poi, marker) {
    if (!map || !infoWindow || !marker) {
      return;
    }

    selectedId = poi.id;
    refreshMarkerSelection();
    updateActiveListItem();

    infoWindow.setContent(
      '<div class="poi-info-window">' +
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

    map.panTo({ lat: Number(poi.lat), lng: Number(poi.lng) });
    if (map.getZoom() < 18) {
      map.setZoom(18);
    }
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
    infoWindow = new google.maps.InfoWindow();

    var canUseAdvanced = !!(config.mapId && config.mapId !== 'DEMO_MAP_ID' && google.maps.marker && google.maps.marker.AdvancedMarkerElement);

    pois.forEach(function (poi) {
      var position = { lat: Number(poi.lat), lng: Number(poi.lng) };
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
        radius: Number(poi.radiusMeters || 10),
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

  function fitVisiblePois() {
    if (!map || !window.google) {
      return;
    }

    var visible = visiblePois();
    if (!visible.length) {
      return;
    }

    var bounds = new google.maps.LatLngBounds();
    visible.forEach(function (poi) {
      bounds.extend({ lat: Number(poi.lat), lng: Number(poi.lng) });
    });

    map.fitBounds(bounds, 88);
    if (visible.length === 1) {
      map.setZoom(18);
    }

    setTimeout(function () {
      if (map && typeof map.setTilt === 'function') {
        map.setTilt(67.5);
      }
    }, 250);
  }

  function applyMarkerVisibility() {
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

        if (entry) {
          setMarkerMap(entry, map);
          if (entry.circle) {
            entry.circle.setMap(map);
          }
          openInfo(poi, entry.marker);
        } else if (map) {
          map.panTo({ lat: Number(poi.lat), lng: Number(poi.lng) });
          map.setZoom(18);
        }
      });

      list.appendChild(button);
    });

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
    var searchInput = byId('poiMapSearch');
    if (searchInput) {
      searchInput.addEventListener('input', function () {
        query = normalize(searchInput.value);
        applyFilters({ fit: false });
      });
    }

    Array.prototype.forEach.call(document.querySelectorAll('[data-poi-status]'), function (button) {
      button.addEventListener('click', function () {
        activeStatus = button.getAttribute('data-poi-status') || 'all';
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

    var tiltButton = byId('poiToggleTilt');
    if (tiltButton) {
      tiltButton.addEventListener('click', function () {
        if (!map || typeof map.setTilt !== 'function') {
          return;
        }

        var currentTilt = Number(map.getTilt() || 0);
        map.setTilt(currentTilt > 0 ? 0 : 67.5);
      });
    }

    var rotateLeft = byId('poiRotateLeft');
    if (rotateLeft) {
      rotateLeft.addEventListener('click', function () {
        if (map && typeof map.setHeading === 'function') {
          map.setHeading(Number(map.getHeading() || 0) - 25);
        }
      });
    }

    var rotateRight = byId('poiRotateRight');
    if (rotateRight) {
      rotateRight.addEventListener('click', function () {
        if (map && typeof map.setHeading === 'function') {
          map.setHeading(Number(map.getHeading() || 0) + 25);
        }
      });
    }
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

  window.gm_authFailure = function () {
    var keyHint = config.apiKeyPrefix ? ' Key: ' + config.apiKeyPrefix + '.' : '';
    showMapMessage('Google Maps dang chan API key hien tai.' + keyHint + ' Trong Google Cloud, hay cho phep key nay dung Maps JavaScript API va HTTP referrer localhost.');
  };

  window.initPoiAdminMap = function () {
    wireUi();
    renderList();

    var mapEl = byId('poiGoogleMap');
    if (!mapEl) {
      return;
    }

    if (!pois.length) {
      showMapMessage('Chua co gian hang nao co toa do hop le.');
      return;
    }

    if (!config.hasApiKey || !window.google || !google.maps) {
      showMapMessage('Can Google Maps API key hop le de hien thi ban do.');
      return;
    }

    var center = config.center || {};
    try {
      var mapOptions = {
        center: {
          lat: Number(center.lat || pois[0].lat),
          lng: Number(center.lng || pois[0].lng)
        },
        zoom: 17,
        tilt: 67.5,
        heading: 25,
        mapTypeId: 'roadmap',
        clickableIcons: false,
        gestureHandling: 'greedy',
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: true,
        rotateControl: true
      };

      if (config.mapId && config.mapId !== 'DEMO_MAP_ID') {
        mapOptions.mapId = config.mapId;
      }

      map = new google.maps.Map(mapEl, mapOptions);

      createMarkers();
      applyFilters({ fit: true });
    } catch (error) {
      showMapMessage(error && error.message ? error.message : 'Google Maps khong khoi tao duoc.');
    }
  };

  document.addEventListener('DOMContentLoaded', function () {
    if (!config.hasApiKey) {
      window.initPoiAdminMap();
    }
  });
})();
