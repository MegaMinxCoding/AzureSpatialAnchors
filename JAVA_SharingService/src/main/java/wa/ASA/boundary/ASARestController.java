package wa.ASA.boundary;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

import wa.ASA.domain.ASAIdentifier;
import wa.ASA.service.IdentService;

@RestController
public class ASARestController {

	@Autowired
	IdentService identService;

	@GetMapping("/getlastanchor")
	public ASAIdentifier getAnchorID() {
		return identService.getLastIdentifier();
	}

	@PostMapping("/addanchor")
	public void addAnchor(@RequestBody ASAIdentifier identifier) {
		identService.addNewIdentifier(identifier);
	}
}
