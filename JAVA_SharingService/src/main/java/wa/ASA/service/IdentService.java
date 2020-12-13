package wa.ASA.service;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import wa.ASA.domain.ASAIdentifier;
import wa.ASA.domain.ASARepository;

@Service
public class IdentService {

	@Autowired
	ASARepository repositpory;

	/**
	 * Adds a new ident to the DataBase
	 * 
	 * @param id
	 */
	public ASAIdentifier addNewIdentifier(ASAIdentifier identifier) {
		identifier.setTimeStamp();
		repositpory.save(identifier);

		return identifier;

	}

	public ASAIdentifier getLastIdentifier() {
		ASAIdentifier newestAnchor = null;
		for (ASAIdentifier ident : repositpory.findAll()) {
			if (newestAnchor == null || ident.getTimeStamp().after(newestAnchor.getTimeStamp())) {
				newestAnchor = ident;
			}
		}
		return newestAnchor;
	}

	public List<ASAIdentifier> getAllIdentifiers() {
		return repositpory.findAll();
	}

	public void deleteAll() {
		repositpory.deleteAll();
	}
}
