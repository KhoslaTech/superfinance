
function AppPermission(perm) {
	perm.Value = perm.PermissionCode;
	perm.DisplayText = perm.Description;
	perm.Implied = [];
	perm.DirectImplied = [];
	perm.Entities = [];
	perm.Key = AppPermission.getKey(perm.PermissionCode);

	perm.getEntities = function (hashedEntities) {
		var list = [];
		for (var i = 0; i < this.Entities.length; i++) {
			var entity = hashedEntities.get(this.EntityTypeCode, this.Entities[i]);
			if (entity)
				list.push(entity);
		}
		return list;
	}

	return perm;
}

AppPermission.translate = function (jsPerm) {
	var hashSet = {};
	for (var i = 0; i < jsPerm.length; i++) {
		var perm = new AppPermission(jsPerm[i]);
		hashSet[perm.Key] = perm;
	}

	hashSet.get = function (code) { return this[AppPermission.getKey(code)]; };

	return hashSet;
};
AppPermission.getKey = function (key) { return key.toUpperCase(); };
AppPermission.isEqual = function (str1, str2) { return AppPermission.getKey(str1) == AppPermission.getKey(str2); };
AppPermission.buildImplied = function (jsPerms, jsImpliedPerms, hashedPerms) {
	for (var i = 0; i < jsPerms.length; i++) {
		traverseImplied(jsPerms[i].PermissionCode, jsPerms[i].Implied, jsPerms[i].DirectImplied);
	}

	function traverseImplied(permissionCode, list, directList) {
		for (var i in jsImpliedPerms) {
			if (AppPermission.isEqual(jsImpliedPerms[i].PermissionCode, permissionCode)) {
				var iPerm = hashedPerms.get(jsImpliedPerms[i].ImpliedPermissionCode);

				// It is possible that the permission being implied is filtered out from being shown, therefore, silently discard it. (An example is user instance permissions based on hierarchy.)
				if (iPerm) {
					if (!list.find(function (x) { return AppPermission.isEqual(x.PermissionCode, iPerm.PermissionCode); })) {
						list.push(iPerm);
					}

					if (directList && !directList.find(function (x) { return AppPermission.isEqual(x.PermissionCode, iPerm.PermissionCode); })) {
						directList.push(iPerm);
					}

					traverseImplied(iPerm.PermissionCode, list);
				}
			}
		}
	}

};
AppPermission.addEntities = function (jsPermitted, hashedPerms) {
	for (var i = 0; i < jsPermitted.length; i++) {
		var permit = jsPermitted[i];
		var perm = hashedPerms.get(permit.PermissionCode);

		addIfNeeded(perm, permit.EntityId);
		//for (var j = 0;j<perm.Implied.length;j++)
		//    addIfNeeded(perm.Implied[j], permit.EntityId );
	}

	function addIfNeeded(perm, entityId) {
		if (!perm.IsAll) {
			if (entityId == null)
				perm.IsAll = true;
			else
				perm.Entities.push(entityId);
		}
	}
};

function AppEntity(entity) {
	entity.Value = entity.Id;
	entity.DisplayText = entity.Name;
	entity.Key = AppEntity.getKey(entity.EntityTypeCode, entity.Id);
	return entity;
}
AppEntity.translate = function (jsEntities) {
	var hashedEntities = {};
	for (var i = 0; i < jsEntities.length; i++) {
		var entity = new AppEntity(jsEntities[i]);
		var typeKey = AppEntity.getKey(entity.EntityTypeCode);
		if (!hashedEntities[typeKey]) {
			hashedEntities[typeKey] = [];
		}

		hashedEntities[typeKey].push(entity);
		hashedEntities[entity.Key] = entity;
	}

	hashedEntities.get = function (typeCode, id) { return this[AppEntity.getKey(typeCode, id)]; }
	return hashedEntities;
};
AppEntity.getKey = function (typeCode, id) {
	return (id ? typeCode + id : typeCode).toUpperCase();
};
