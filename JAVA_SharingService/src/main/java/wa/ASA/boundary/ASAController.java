package wa.ASA.boundary;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;

import wa.ASA.domain.ASAIdentifier;
import wa.ASA.service.IdentService;

@Controller
public class ASAController {

	@Autowired
	IdentService service;

	@RequestMapping(value = { "/anchors", "/" })
	public String listAllGrades(Model model) {
		model.addAttribute("listAllAnchors", service.getAllIdentifiers());
		return "anchors";
	}

	@RequestMapping(value = "/addAnchorManual", method = RequestMethod.POST)
	public String addAnchorManual(@RequestParam String id, Model model) {
		service.addNewIdentifier(new ASAIdentifier(id));
		System.out.println(service.getAllIdentifiers().size());
		return "redirect:anchors";
	}

	@RequestMapping(value = "/deleteAll", method = RequestMethod.POST)
	public String deleteAll(Model model) {
		service.deleteAll();
		return "redirect:anchors";
	}

}
