def get():
	import OleViewDotNet

	return OleViewDotNet.COMRegistry.Instance
	
def localservices():
	reg = get()
	appids = {k: v for k, v in dict(reg.AppIDs).iteritems() if v.LocalService is not None }
	
	for g in reg.ClsidsByAppId:
		if g.Key in appids:
			for c in g:
				yield c